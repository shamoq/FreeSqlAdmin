using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Utils.Extensions;
using System.Data;
using System.Reflection;
using Simple.Utils;
using Simple.Utils.Generator;
using Simple.Utils.Generator.Dto;

namespace Simple.Utils.Generator.Dialect
{
    public class MysqlDatabaseInfo : IDatabaseInfo
    {
        private readonly IDbConnection dbConnection;
        private List<DatabaseTableInfoDto> allTables;

        public MysqlDatabaseInfo(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo)
        {
            var tableName = tableInfo.Name;
            var fieldInfos = tableInfo.FieldInfos;
            var schemaTableName = GetSchemaTableName(tableInfo.Schema, tableName);
            var body = new StringBuilder();
            body.AppendLine($"CREATE TABLE {schemaTableName} (");
            //主键
            var keyField = "";
            var hasKeyField = fieldInfos.Any(it => it.IsKey);

            for (int i = 0; i < fieldInfos.Count; i++)
            {
                var fieldInfo = fieldInfos[i];

                //行末尾是否有逗号
                var lastComma = "";
                if (i != fieldInfos.Count - 1)
                {
                    lastComma = ",";
                }
                else
                {
                    lastComma = hasKeyField ? "," : "";
                }

                body.AppendLine($"    {GetFieldChangeSql(fieldInfo, false)}{lastComma}");
                if (fieldInfo.IsKey)
                {
                    keyField = fieldInfo.ColumnName;
                }
            }

            if (!string.IsNullOrEmpty(keyField))
            {
                body.AppendLine($"    PRIMARY KEY (`{keyField}`)");
            }

            body.AppendLine($")");

            // 表注释
            if (!string.IsNullOrEmpty(tableInfo.Description))
            {
                body.Append($"  COMMENT = '{tableInfo.Description}'");
            }

            var result = new GenerateDatabaseSqlResult()
            {
                Body = body.ToString(),
            };

            return result;
        }

        /// <summary>
        /// 判断字段是否发生变化
        /// </summary>
        /// <returns></returns>
        public bool IsFieldChange(DatabaseFieldInfoDto typeField, DatabaseFieldInfoDto dbField)
        {
            var isChange = false;

            // 列注释变更
            isChange = isChange || !string.IsNullOrEmpty(typeField.Description) &&
                dbField.Description != typeField.Description;

            // 如果指定了最大长度，只需要比对类型是否是text
            if (typeField.DataType == "varchar" && typeField.StringMaxLength == "max")
            {
                isChange = isChange || dbField.DataType != "text";
            }
            else
            {
                isChange = isChange || dbField.DataType != typeField.DataType;
            }

            // 字符串列长度类型变更
            if (typeField.StringMaxLength == "max")
            {
                // 如果是指定了max，长度超过4000就不修改了，mysql是 65536
                int.TryParse(dbField.StringMaxLength, out var len);
                isChange = isChange || len < 4000;
            }
            else
            {
                isChange = isChange || dbField.StringMaxLength != typeField.StringMaxLength;
            }

            // 数值精度变更
            isChange = isChange || dbField.Precision != typeField.Precision;
            isChange = isChange || dbField.Scale != typeField.Scale;

            isChange = isChange || dbField.IsNullable != typeField.IsNullable;

            return isChange;
        }

        /// <summary>
        /// 通过字段信息生成生成表的sql
        /// </summary>
        /// <returns></returns>
        private string GetFieldChangeSql(DatabaseFieldInfoDto fieldInfo, bool isAlter)
        {
            var identityString = fieldInfo.IsAutoCreate && fieldInfo.IsKey ? "AUTO_INCREMENT" : "";
            var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
            var primaryKeyString = fieldInfo.IsAutoCreate && fieldInfo.IsKey && isAlter ? "PRIMARY KEY" : "";
            var columnDataType = fieldInfo.DataType;
            if (fieldInfo.DataType == "char" || fieldInfo.DataType == "varchar")
            {
                if (fieldInfo.StringMaxLength == "max")
                {
                    columnDataType = "text";
                }
                else
                {
                    columnDataType = $"{fieldInfo.DataType}({fieldInfo.StringMaxLength})";
                }
            }
            //自定义decimal精度类型
            if (fieldInfo.DataType == "decimal")
            {
                columnDataType =
                    $"decimal({fieldInfo.Precision},{fieldInfo.Scale})";
            }
            var columnName = fieldInfo.ColumnName;
            var result = $"{columnName} {columnDataType}";
            if (!string.IsNullOrEmpty(nullableString))
            {
                result += $" {nullableString}";
            }
            if (!string.IsNullOrEmpty(primaryKeyString))
            {
                result += $" {primaryKeyString}";
            }
            if (!string.IsNullOrEmpty(identityString))
            {
                result += $" {identityString}";
            }
            if (!string.IsNullOrEmpty(fieldInfo.Description))
            {
                result += $" comment '{fieldInfo.Description}' ";
            }
            return result;
        }

        public string CreateTableDescription(string schema, string tableName, string description)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} COMMENT = '{description}'";
            return sql;
        }

        public string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} ADD {GetFieldChangeSql(fieldInfo, true)}";
            return sql;
        }

        public string DropTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} drop column {fieldInfo.ColumnName} ";
            return sql;
        }

        public string UpdateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            var schemaTableName = GetSchemaTableName(schema, tableName);
            var sql = $"ALTER TABLE {schemaTableName} MODIFY {GetFieldChangeSql(fieldInfo, false)} COMMENT '{fieldInfo.Description}'";
            return sql;
        }

        public DatabaseTableInfoDto GetTableInfoByName(string schema, string tableName)
        {
            schema = GetDefaultSchema(schema);
            if (allTables == null)
            {
                var sql = @"
                       SELECT
                            TABLE_NAME AS TableName,
                            COLUMN_NAME AS columnName,
                            ORDINAL_POSITION AS '列的排列顺序',
                            COLUMN_DEFAULT AS '默认值',
                            CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END as isNullable,
                            DATA_TYPE AS DataType,
                            CHARACTER_MAXIMUM_LENGTH AS StringMaxLength,
                            NUMERIC_PRECISION AS 'precision',
                            NUMERIC_SCALE AS 'Scale',
                            COLUMN_TYPE AS DataFullType,
                            COLUMN_COMMENT AS Description,
                            CASE WHEN COLUMN_KEY = 'PRI' THEN 1 ELSE 0 END as IsKey,
                            CASE WHEN EXTRA = 'auto_increment' THEN 1 ELSE 0 END AS isAutoCreate
                                            FROM
                        information_schema.`COLUMNS`
                    WHERE
                        TABLE_SCHEMA = @schemaName
                    ORDER BY
                        TABLE_NAME,
                        ORDINAL_POSITION ";
                var fieldInfos = dbConnection.Query<DatabaseFieldInfoDto>(sql, new { schemaName = schema }).ToList();

                var tableDescriptionSql = @"SELECT
                                        TABLE_NAME AS Name,
                                        TABLE_COMMENT AS Description
                                        FROM information_schema.tables
                                        WHERE TABLE_SCHEMA =@schemaName ";

                var tables = dbConnection.Query<DatabaseTableInfoDto>(tableDescriptionSql, new { schemaName = schema });

                foreach (var table in tables)
                {
                    table.FieldInfos = fieldInfos.Where(t => t.TableName == table.Name).ToList();
                    // 处理特殊场景mysql int会默认有精度10，这里清零，保持后面比较正确
                    foreach (var field in table.FieldInfos)
                    {
                        if (field.DataType == "int")
                        {
                            field.Precision = 0;
                        }
                    }
                }

                allTables = tables;
            }
            return allTables.FirstOrDefault(t => string.Compare(t.Name, tableName, true) == 0);
        }

        public string CreatePrimaryKey(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
        {
            //var schemaTableName = GetSchemaTableName(schema, tableName);
            //var sql =
            //    $"ALTER TABLE {schemaTableName} ADD CONSTRAINT {tableName}_PK PRIMARY KEY({BoxTableNameOrColumnName(fieldInfo.ColumnName)})";

            //return sql;
            return "";
        }

        public string GetSchemaTableName(string schema, string tableName)
        {
            tableName = !string.IsNullOrEmpty(schema) ? schema + "." + tableName : tableName;
            return tableName;
        }

        public string GetDefaultSchema(string schema)
        {
            if (!string.IsNullOrEmpty(schema))
            {
                return schema;
            }
            return dbConnection.Database;
        }

        public List<string> GetAllTableNames()
        {
            var sql = @"SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = DATABASE()";

            var tableNames = dbConnection.Query<string>(sql).ToList();
            return tableNames;
        }

        private Dictionary<string, string> csharpTypeToDatabaseTypeMappings = new Dictionary<string, string>()
        {
            {"String","varchar"},
            {"Int32","int"},
            {"Int64","bigint"},
            {"DateTime","datetime"},
            {"Decimal","decimal"},
            {"Boolean","int"},
            {"Byte[]","binary"},
            {"Double","double"},
            {"Int16","smallint"},
            {"TimeSpan","time"},
            {"Guid","char(36)"},
            {"Byte","tinyint unsigned"},
            {"Single","float"},
        };

        private Dictionary<string, string> DatabaseTypeToCsharpTypeMappings = new Dictionary<string, string>()
        {
            {"bigint","long"},
            {"bigint unsigned","ulong"},
            {"blob","byte[]"},
            {"date","DateTime"},
            {"datetime","DateTime"},
            {"double","double"},
            {"float","float"},
            {"int","int"},
            {"int unsigned","uint"},
            {"mediumblob","byte[]"},
            {"mediumtext","string"},
            {"longblob","byte[]"},
            {"longtext","string"},
            {"mediumint","int"},
            {"mediumint unsigned","uint"},
            {"smallint","short"},
            {"smallint unsigned","ushort"},
            {"text","string(max)"},
            {"time","TimeSpan"},
            {"timestamp","DateTime"},
            {"tinyblob","byte[]"},
            {"tinyint","sbyte"},
            {"tinyint unsigned","byte"},
            {"tinytext","string"},
            {"varchar","string"},
            {"year","short"},
            {"json","string"},
        };

        public string ConvertCsharpTypeToDatabaseType(string csharpType)
        {
            if (csharpTypeToDatabaseTypeMappings.TryGetValue(csharpType, out var dbType))
            {
                return dbType;
            }

            throw new Exception($"can not convert {csharpType} to database type");
        }

        public string ConvertDatabaseTypeToCsharpType(DatabaseFieldInfoDto databaseFieldInfo)
        {
            var item = "";
            if (DatabaseTypeToCsharpTypeMappings.ContainsKey(databaseFieldInfo.DataType))
            {
                item = DatabaseTypeToCsharpTypeMappings[databaseFieldInfo.DataType];
            }

            if (IsType(databaseFieldInfo, "binary") || IsType(databaseFieldInfo, "varbinary"))
            {
                item = "byte[]";
            }
            //特殊情况
            if (databaseFieldInfo.DataType == "varbinary(16)" || databaseFieldInfo.DataType == "char(36)")
            {
                item = "Guid";
            }
            if (IsType(databaseFieldInfo, "bit"))
            {
                item = "ulong";
            }
            if (IsType(databaseFieldInfo, "tinyint(1)"))
            {
                item = "bool";
            }
            if (IsType(databaseFieldInfo, "char") || IsType(databaseFieldInfo, "varchar"))
            {
                item = "string";
            }
            if (IsType(databaseFieldInfo, "decimal"))
            {
                item = "decimal";
            }

            if (string.IsNullOrWhiteSpace(item))
            {
                throw new NotSupportedException("not support " + databaseFieldInfo.DataType);
            }

            return item;
        }

        private bool IsType(DatabaseFieldInfoDto dto, string dataTypeName)
        {
            if (dto.DataType.Contains(dataTypeName) && dto.DataType.Substring(0, dataTypeName.Length) == dataTypeName)
            {
                return true;
            }

            return false;
        }
    }
}