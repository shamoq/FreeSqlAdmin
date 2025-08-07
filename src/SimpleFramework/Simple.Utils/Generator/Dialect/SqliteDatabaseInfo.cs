//using SummerBoot.Repository.Generator.Dto;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Data;
//using Simple.Utils.Extensions;
//using SummerBoot.Repository.Generator;

//namespace Simple.AdminApplication.Generator.Dialect
//{
//    public class SqliteDatabaseInfo : IDatabaseInfo
//    {
//        private readonly IDbConnection dbConnection;

//        public SqliteDatabaseInfo(IDbConnection dbConnection)
//        {
//            this.dbConnection = dbConnection;
//        }

//        public GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo)
//        {
//            var tableName = tableInfo.Name;
//            var fieldInfos = tableInfo.FieldInfos;

//            var body = new StringBuilder();
//            body.AppendLine($"CREATE TABLE {tableName} (");

//            var hasKeyField = fieldInfos.Any(it => it.IsKey);

//            for (int i = 0; i < fieldInfos.Count; i++)
//            {
//                var fieldInfo = fieldInfos[i];

//                //行末尾是否有逗号
//                var lastComma = "";
//                if (i != fieldInfos.Count - 1)
//                {
//                    lastComma = ",";
//                }

//                body.AppendLine($"    {GetCreateFieldSqlByFieldInfo(fieldInfo)}{lastComma}");
//            }

//            body.AppendLine($")");

//            var result = new GenerateDatabaseSqlResult()
//            {
//                Body = body.ToString(),
//                Descriptions = new List<string>(),
//                FieldModifySqls = new List<string>()
//            };

//            return result;
//        }

//        /// <summary>
//        /// 通过字段信息生成生成表的sql
//        /// </summary>
//        /// <param name="fieldInfo"></param>
//        /// <returns></returns>
//        private string GetCreateFieldSqlByFieldInfo(DatabaseFieldInfoDto fieldInfo)
//        {
//            var identityString = fieldInfo.IsAutoCreate ? "AUTOINCREMENT" : "";
//            var nullableString = fieldInfo.IsNullable ? "NULL" : "NOT NULL";
//            var pk = fieldInfo.IsKey ? "PRIMARY KEY" : "";
//            var columnDataType = fieldInfo.ColumnDataType;

//            if (!string.IsNullOrEmpty(fieldInfo.SpecifiedColumnDataType))
//            {
//                columnDataType = fieldInfo.SpecifiedColumnDataType;
//            }

//            var columnName = fieldInfo.ColumnName;
//            var result = $"{columnName} {columnDataType}";
//            if (!string.IsNullOrEmpty(nullableString))
//            {
//                result += $" {nullableString}";
//            }
//            if (!string.IsNullOrEmpty(pk))
//            {
//                result += $" {pk}";
//            }
//            if (!string.IsNullOrEmpty(identityString))
//            {
//                result += $" {identityString}";
//            }
//            //var result = $"{columnName} {columnDataType} {nullableString}{pk}{identityString}";
//            return result;
//        }

//        public string CreateTableDescription(string schema, string tableName, string description)
//        {
//            return "";
//        }

//        public string UpdateTableDescription(string schema, string tableName, string description)
//        {
//            return "";
//        }

//        public string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
//        {
//            var sql = $"ALTER TABLE {tableName} ADD {GetCreateFieldSqlByFieldInfo(fieldInfo)}";
//            return sql;
//        }

//        public string CreateTableFieldDescription(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
//        {
//            return "";
//        }

//        public string UpdateTableFieldDescription(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
//        {
//            return "";
//        }

//        public DatabaseTableInfoDto GetTableInfoByName(string schema, string tableName)
//        {
//            var sql = @"SELECT sql FROM sqlite_master WHERE tbl_name = @tableName limit 2";
//            var fieldInfos = new List<DatabaseFieldInfoDto>();

//            var tableStruct = dbConnection.Query<string>(sql, new { tableName }).FirstOrDefault();
//            if (!string.IsNullOrEmpty(tableStruct))
//            {
//                var tableStructArr = tableStruct.Split(Environment.NewLine).ToList();
//                foreach (var line in tableStructArr)
//                {
//                    if (line.Contains("CREATE TABLE") || line == ")")
//                    {
//                        continue;
//                    }
//                    var fieldInfo = new DatabaseFieldInfoDto()
//                    {
//                        IsNullable = true,
//                        IsKey = false,
//                        IsAutoCreate = false,
//                    };

//                    var tempLine = line.Trim();

//                    var lineArr = tempLine.Split(" ").ToList();
//                    var hasFieldName = false;
//                    foreach (var tempLinePart in lineArr)
//                    {
//                        var matchValue = Regex.Match(tempLinePart, "\"[^\"}]*\"");
//                        if (matchValue.Success)
//                        {
//                            hasFieldName = true;
//                            fieldInfo.ColumnName = matchValue.Value.Replace("\"", "");
//                            continue;
//                        }

//                        if (hasFieldName)
//                        {
//                            fieldInfo.ColumnDataType = tempLinePart;
//                            break;
//                        }
//                    }

//                    if (tempLine.Contains("NOT NULL"))
//                    {
//                        fieldInfo.IsNullable = false;
//                    }

//                    if (tempLine.Contains("PRIMARY KEY"))
//                    {
//                        fieldInfo.IsKey = true;
//                    }
//                    if (tempLine.Contains("AUTOINCREMENT"))
//                    {
//                        fieldInfo.IsAutoCreate = true;
//                    }
//                    fieldInfos.Add(fieldInfo);
//                }
//            }

//            var result = new DatabaseTableInfoDto()
//            {
//                Name = tableName,
//                Description = "",
//                FieldInfos = fieldInfos
//            };

//            return result;
//        }

//        public string CreatePrimaryKey(string schema, string tableName, DatabaseFieldInfoDto fieldInfo)
//        {
//            throw new NotImplementedException();
//        }

//        public string GetSchemaTableName(string schema, string tableName)
//        {
//            throw new NotImplementedException();
//        }

//        public string GetDefaultSchema(string schema)
//        {
//            return "";
//        }

//        public List<string> GetAllTableNames()
//        {
//            var sql = @"SELECT name
//                FROM sqlite_master
//                WHERE type = 'table'
//                AND name NOT LIKE 'sqlite_%'";

//            var tableNames = dbConnection.Query<string>(sql).ToList();
//            return tableNames;
//        }

//        private Dictionary<string, string> csharpTypeToDatabaseTypeMappings = new Dictionary<string, string>()
//        {
//            {"String","TEXT"},
//            {"Int32","INTEGER"},
//            {"Int64","INTEGER"},
//            {"DateTime","TEXT"},
//            {"Decimal","TEXT"},
//            {"Boolean","INTEGER"},
//            {"Byte[]","BLOB"},
//            {"Double","REAL"},
//            {"Int16","INTEGER"},
//            {"TimeSpan","TEXT"},
//            {"Guid","TEXT"},
//            {"Byte","INTEGER"},
//            {"Single","REAL"},
//        };

//        private Dictionary<string, string> DatabaseTypeToCsharpTypeMappings = new Dictionary<string, string>()
//        {
//            {"TEXT","string"},
//            {"BLOB","byte[]"},
//            {"NUMERIC","byte[]"},
//            {"INTEGER","long"},
//            {"REAL","double"},
//        };

//        public string ConvertCsharpTypeToDatabaseType(string csharpTypeList)
//        {
//            var result = new List<string>();
//            foreach (var type in csharpTypeList)
//            {
//                var item = csharpTypeToDatabaseTypeMappings[type];
//                result.Add(item);
//            }

//            return result;
//        }

//        public List<string> ConvertDatabaseTypeToCsharpType(List<DatabaseFieldInfoDto> databaseFieldInfoList)
//        {
//            var result = new List<string>();
//            foreach (var databaseFieldInfo in databaseFieldInfoList)
//            {
//                var item = "";
//                if (DatabaseTypeToCsharpTypeMappings.ContainsKey(databaseFieldInfo.ColumnDataType))
//                {
//                    item = DatabaseTypeToCsharpTypeMappings[databaseFieldInfo.ColumnDataType];
//                }

//                if (!string.IsNullOrEmpty(item))
//                {
//                    result.Add(item);
//                }
//                else
//                {
//                    throw new NotSupportedException(databaseFieldInfo.ColumnDataType);
//                }
//            }

//            return result;
//        }
//    }
//}