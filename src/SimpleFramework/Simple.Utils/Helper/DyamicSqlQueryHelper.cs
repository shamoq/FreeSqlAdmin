using Newtonsoft.Json.Linq;
using Simple.Utils.Models.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Simple.Utils.Helper
{
    public class DyamicSqlQueryHelper
    {
        private static IDbCommand Prepare(IDbConnection dbConnection, SqlQueryHelperContext context)
        {
            var sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XmlCommand", context.SqlName);
            var sql = File.ReadAllText(sqlFilePath);

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandText = sql;
                if (context.Parameters != null)
                {
                    foreach (var param in context.Parameters)
                    {
                        var para = command.CreateParameter();
                        para.ParameterName = param.Key;
                        para.Value = param.Value;
                        command.Parameters.Add(para);
                    }
                }
                return command;
            }
        }

        public static T ExecuteScalar<T>(IDbConnection dbConnection, SqlQueryHelperContext context)
        {
            using (var command = Prepare(dbConnection, context))
            {
                try
                {
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        dbConnection.Open();
                    }
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return (T)Convert.ChangeType(result, typeof(T));
                    }
                    throw new NullReferenceException();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (dbConnection.State == ConnectionState.Open)
                    {
                        dbConnection.Close();
                    }
                }
            }
        }

        public static List<T> ExecuteSql<T>(IDbConnection dbConnection, SqlQueryHelperContext context)
        {
            using (var command = Prepare(dbConnection, context))
            {
                try
                {
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        dbConnection.Open();
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        var resultList = new List<T>();
                        var properties = typeof(T).GetProperties();

                        if (typeof(T) == typeof(Dictionary<string, object>))
                        {
                            while (reader.Read())
                            {
                                var dict = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    dict[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                                resultList.Add((T)(object)dict);
                            }
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                var instance = Activator.CreateInstance<T>();
                                if (instance is BaseDto dto)
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        dto.SetAttributeValue(reader.GetName(i), reader.GetValue(i));
                                    }
                                }
                                else
                                {
                                    foreach (var property in properties)
                                    {
                                        if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                                        {
                                            var value = reader.GetValue(reader.GetOrdinal(property.Name));
                                            property.SetValue(instance, Convert.ChangeType(value, property.PropertyType));
                                        }
                                    }
                                }
                                resultList.Add(instance);
                            }
                        }

                        return resultList;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (dbConnection.State == ConnectionState.Open)
                    {
                        dbConnection.Close();
                    }
                }
            }
        }
    }

    public class SqlQueryHelperContext
    {
        public string SqlName { get; set; }

        public Dictionary<string, object> Parameters { get; set; }
    }
}