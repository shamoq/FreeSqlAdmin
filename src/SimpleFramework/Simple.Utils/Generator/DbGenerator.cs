using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using Simple.Utils;
using Simple.Utils.Comment;
using Simple.Utils.Extensions;
using Simple.Utils.Generator.Core;
using Simple.Utils.Generator.Dto;
using Simple.Utils.Helper;

namespace Simple.Utils.Generator
{
    public class DbGenerator
    {
        private readonly IDatabaseInfo databaseInfo;
        private bool isDev;

        public DbGenerator(IDatabaseInfo databaseInfo, bool isDev)
        {
            this.databaseInfo = databaseInfo;
            this.isDev = isDev;
        }

        public void Upgrade(DbConnection dbConnection, Type entityType)
        {
            var time1 = DateTime.Now;

            // 获取继承自 DefaultEntity 的所有类型
            List<Type> derivedTypes = RuntimeHelper.GetAllTypes(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(entityType));

            var generateDatabaseSqlResults = GenerateSql(derivedTypes);

            foreach (var result in generateDatabaseSqlResults)
            {
                ExecuteGenerateSql(dbConnection, result);
            }

            Console.WriteLine("数据库升级完成: 耗时" + (DateTime.Now - time1).TotalSeconds + "s");
        }

        public void ExecuteGenerateSql(DbConnection dbConnection, GenerateDatabaseSqlResult generateDatabaseSqlResult)
        {
            if (!string.IsNullOrEmpty(generateDatabaseSqlResult.Body))
            {
                dbConnection.Execute(generateDatabaseSqlResult.Body, close: false);
            }

            foreach (var fieldModifySql in generateDatabaseSqlResult.FieldModifySqls)
            {
                dbConnection.Execute(fieldModifySql, close: false);
            }

            dbConnection.Close();
        }

        /// <summary>
        /// 需要处理的实体类型
        /// 不处理 dictionary，class，list
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool IsEntityType(PropertyInfo property)
        {
            Type propertyType = property.PropertyType;

            // 如果是可空类型，继续处理
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }
            // 如果是其他泛型类型（集合等），则跳过
            else if (propertyType.IsGenericType)
            {
                return false;
            }
            // 如果是类类型，则跳过
            else if (propertyType != typeof(string) && propertyType.IsClass)
            {
                return false;
            }

            var notMapper = propertyType.GetCustomAttribute<NotMappedAttribute>();
            if (notMapper != null)
            {
                return false;
            }

            return propertyType.IsValueType ||
                   propertyType == typeof(string) ||
                   propertyType == typeof(DateTime) ||
                   propertyType == typeof(DateTimeOffset) ||
                   propertyType == typeof(TimeSpan) ||
                   propertyType == typeof(Guid);
        }

        public List<GenerateDatabaseSqlResult> GenerateSql(List<Type> types)
        {
            var result = new List<GenerateDatabaseSqlResult>();
            foreach (var type in types)
            {
                var tableAttribute = type.GetCustomAttribute<TableAttribute>();
                var tableDescriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
                var tableName = tableAttribute != null ? tableAttribute.Name : type.Name;

                var schema = tableAttribute?.Schema;
                schema = databaseInfo.GetDefaultSchema(schema);
                var tableDescription = tableDescriptionAttribute?.Description ?? "";

                // 尝试读取注释
                if (string.IsNullOrEmpty(tableDescription))
                {
                    tableDescription = CSCommentReader.Create(type)?.Summary;
                }

                if (tableName.ToLower().Contains("richtextdata"))
                {
                }

                // 过滤需要处理的实体类型
                var propertys = type.GetProperties()
                    .Where(IsEntityType)
                    .OrderBy(it => it.ReflectedType == it.DeclaringType ? 1 : 0)
                    .ToList();
                var fieldInfos = new List<DatabaseFieldInfoDto>();
                foreach (var propertyInfo in propertys)
                {
                    var fieldInfo = new DatabaseFieldInfoDto()
                    {
                        ColumnType = propertyInfo.PropertyType,
                        IsKey = propertyInfo.GetCustomAttribute<KeyAttribute>() != null,
                        ColumnName = propertyInfo.GetCustomAttribute<ColumnAttribute>()?.Name ?? propertyInfo.Name,
                    };

                    if (fieldInfo.ColumnName.Equals("DataValue", StringComparison.OrdinalIgnoreCase))
                    {
                    }

                    // 定义必填就非空
                    if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null)
                    {
                        fieldInfo.IsNullable = false;
                    }
                    else if (propertyInfo.PropertyType.IsNullable())
                    {
                        fieldInfo.IsNullable = true;
                    }
                    if (fieldInfo.IsKey)
                    {
                        fieldInfo.IsNullable = false;
                    }
                    //如果是string类型，允许可空
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        fieldInfo.IsNullable = true;
                    }
                    // 如果是guid，允许可空
                    else if (propertyInfo.PropertyType == typeof(Guid) && !fieldInfo.IsKey)
                    {
                        fieldInfo.IsNullable = true;
                    }

                    //如果是枚举类型，统一转为int
                    if (TypeExtension.GetNotNullType(propertyInfo.PropertyType).IsEnum)
                    {
                        fieldInfo.ColumnType = typeof(int);
                    }

                    var databaseGeneratedAttribute = propertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
                    fieldInfo.IsAutoCreate = (propertyInfo.PropertyType == typeof(int) ||
                                              propertyInfo.PropertyType == typeof(long)) &&
                                             databaseGeneratedAttribute != null &&
                                             databaseGeneratedAttribute.DatabaseGeneratedOption ==
                                             DatabaseGeneratedOption.Identity;

                    // 获取注释
                    var descriptionAttribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
                    fieldInfo.Description = descriptionAttribute != null ? descriptionAttribute.Description : "";

                    if (string.IsNullOrEmpty(fieldInfo.Description))
                    {
                        // 判断类型是否在当前类中定义的
                        if (propertyInfo.DeclaringType.Name == type.Name)
                        {
                            fieldInfo.Description = CSCommentReader.Create(propertyInfo)?.Summary;
                        }
                        else
                        {
                            var prop = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name);
                            fieldInfo.Description = CSCommentReader.Create(prop)?.Summary;
                        }
                    }

                    // 获取dbTypeName,其中可能有空格
                    var dbFieldTypeName = propertyInfo.GetCustomAttribute<ColumnAttribute>()?.TypeName ??
                                          databaseInfo.ConvertCsharpTypeToDatabaseType(TypeExtension
                                              .GetNotNullType(propertyInfo.PropertyType).Name);

                    fieldInfo.DataFullType = dbFieldTypeName;
                    // 解析字符串长度
                    var regex = new Regex(@"^(\w+)(?:\(([\d,]+)\))?$");
                    var match = regex.Match(dbFieldTypeName);
                    if (match.Success)
                    {
                        fieldInfo.DataType = match.Groups[1].Value;

                        string numbersStr = match.Groups[2].Value;
                        var numbers = numbersStr.Split(',').Where(t => !string.IsNullOrEmpty(t)).ToList();
                        if (numbers.Count == 1)
                        {
                            // 字符串列
                            fieldInfo.StringMaxLength = numbers[0];
                        }
                        else if (numbers.Count == 2)
                        {
                            // 数值列
                            fieldInfo.Precision = int.Parse(numbers[0]);
                            fieldInfo.Scale = int.Parse(numbers[1]);
                        }
                    }

                    //decimal精度问题，默认18,2
                    if (propertyInfo.PropertyType == typeof(decimal))
                    {
                        var decimalPrecisionAttribute = propertyInfo.GetCustomAttribute<DecimalPrecisionAttribute>();
                        if (decimalPrecisionAttribute != null)
                        {
                            fieldInfo.Precision = decimalPrecisionAttribute.Precision;
                            fieldInfo.Scale = decimalPrecisionAttribute.Scale;
                        }
                        else
                        {
                            fieldInfo.Precision = 18;
                            fieldInfo.Scale = 2;
                        }
                    }

                    // 字符串长度默认值 1024
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        var stringTypeLength = "1024";
                        var stringLength = propertyInfo.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength;
                        if (stringLength != null)
                        {
                            if (stringLength == -1 || stringLength == int.MaxValue)
                            {
                                stringTypeLength = "max";
                            }
                            else
                            {
                                stringTypeLength = stringLength.ToString();
                            }
                        }

                        fieldInfo.StringMaxLength = stringTypeLength;
                    }

                    fieldInfos.Add(fieldInfo);
                }

                //判断数据库中是否已经有这张表，如果有，则比较两张表的结构
                var dbTableInfo = databaseInfo.GetTableInfoByName(schema, tableName);
                //如果存在这张表，判断字段新增和修改
                if (dbTableInfo != null && dbTableInfo.FieldInfos.Any())
                {
                    var item = new GenerateDatabaseSqlResult();

                    // 有注释并且和数据库的注释不相等
                    if (!string.IsNullOrEmpty(tableDescription) && tableDescription != dbTableInfo.Description)
                    {
                        var newTableDescriptionSql =
                            databaseInfo.CreateTableDescription(schema, tableName, tableDescription);
                        if (!string.IsNullOrEmpty(newTableDescriptionSql))
                        {
                            item.FieldModifySqls.Add(newTableDescriptionSql);
                        }
                    }

                    foreach (var fieldInfo in fieldInfos)
                    {
                        var dbFieldInfo =
                            dbTableInfo.FieldInfos.FirstOrDefault(it => it.ColumnName == fieldInfo.ColumnName);
                        if (dbFieldInfo == null)
                        {
                            var createFieldSql = databaseInfo.CreateTableField(schema, tableName, fieldInfo);
                            item.FieldModifySqls.Add(createFieldSql);

                            //添加约束
                            if (fieldInfo.IsKey)
                            {
                                var createPrimaryKeySql = databaseInfo.CreatePrimaryKey(schema, tableName, fieldInfo);

                                if (!string.IsNullOrEmpty(createPrimaryKeySql))
                                {
                                    item.FieldModifySqls.Add(createPrimaryKeySql);
                                }
                            }
                        }
                        else
                        {
                            // 判断列是否需要变更
                            bool isChange = databaseInfo.IsFieldChange(fieldInfo, dbFieldInfo);

                            if (isChange)
                            {
                                var fieldChangeSql = databaseInfo.UpdateTableField(schema, tableName, fieldInfo);
                                if (!string.IsNullOrEmpty(fieldChangeSql))
                                {
                                    item.FieldModifySqls.Add(fieldChangeSql);
                                }
                            }
                        }
                    }

                    result.Add(item);
                }
                else
                {
                    //不存在这张表则新建
                    var item = databaseInfo.CreateTable(new DatabaseTableInfoDto()
                    {
                        Description = tableDescription,
                        Name = tableName,
                        FieldInfos = fieldInfos,
                        Schema = schema
                    });
                    result.Add(item);
                }

                // 检查数据库的字段是否都存在，不存在的字段,开发环境一律删除，其他环境设置列为可空
                var notExistsFields = dbTableInfo?.FieldInfos.Where(it => !fieldInfos.Any(f => f.ColumnName.Equals(it.ColumnName, StringComparison.OrdinalIgnoreCase)))
                            .ToList() ?? new List<DatabaseFieldInfoDto>();

                // 删除不存在的字段
                if (isDev)
                {
                    foreach (var dbField in notExistsFields)
                    {
                        var item = new GenerateDatabaseSqlResult();

                        var dropFieldSql = databaseInfo.DropTableField(schema, tableName, dbField);
                        if (!string.IsNullOrEmpty(dropFieldSql))
                        {
                            item.FieldModifySqls.Add(dropFieldSql);
                            result.Add(item);
                        }
                    }
                }
                else
                {
                    foreach (var dbField in notExistsFields)
                    {
                        var item = new GenerateDatabaseSqlResult();
                        dbField.IsNullable = true;
                        var fieldChangeSql = databaseInfo.UpdateTableField(schema, tableName, dbField);
                        if (!string.IsNullOrEmpty(fieldChangeSql))
                        {
                            item.FieldModifySqls.Add(fieldChangeSql);
                            result.Add(item);
                        }
                    }
                }
            }

            return result;
        }
    }
}