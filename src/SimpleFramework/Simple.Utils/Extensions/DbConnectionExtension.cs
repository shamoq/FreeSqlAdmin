using Simple.Utils.Helper;
using Simple.Utils.Models.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Simple.Utils.Extensions
{
    public static class DbConnectionExtension
    {
        private static IDbCommand Prepare(IDbConnection conn, string sql, object parameters = null)
        {
            ConsoleHelper.Info(sql);

            var command = conn.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                if (parameters is IDictionary<string, object> dictionary)
                {
                    foreach (var param in dictionary)
                    {
                        ConsoleHelper.Info($"{param.Key}:{param.Value} ");
                        var para = command.CreateParameter();
                        para.ParameterName = param.Key;
                        para.Value = param.Value ?? DBNull.Value;
                        command.Parameters.Add(para);
                    }
                }
                else
                {
                    foreach (var property in parameters.GetType().GetProperties())
                    {
                        var paramName = property.Name;
                        var paramValue = property.GetValue(parameters, null);
                        ConsoleHelper.Info($"{paramName}:{paramValue} ");
                        var para = command.CreateParameter();
                        para.ParameterName = paramName;
                        para.Value = paramValue ?? DBNull.Value;
                        command.Parameters.Add(para);
                    }
                }
            }

            return command;
        }

        public static T ExecuteScalar<T>(this IDbConnection conn, string sql, object parameters = null, bool close = true)
        {
            using (var command = Prepare(conn, sql, parameters))
            {
                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
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
                    if (close && conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
        }

        public static List<T> Query<T>(this IDbConnection conn, string sql, object parameters = null, bool close = true)
        {
            using (var command = Prepare(conn, sql, parameters))
            {
                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
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
                        else if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || typeof(T) == typeof(DateTime))
                        {
                            while (reader.Read())
                            {
                                var value = TypeConvertHelper.ConvertType<T>(reader.GetValue(0));
                                resultList.Add(value);
                            }
                        }
                        else
                        {
                            // 创建一个 Dictionary，使用 StringComparer.OrdinalIgnoreCase 忽略键的大小写
                            Dictionary<string, int> colNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                colNames[reader.GetName(i)] = i;
                            }

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
                                        if (colNames.TryGetValue(property.Name, out var index))
                                        {
                                            if (!reader.IsDBNull(index))
                                            {
                                                var value = reader.GetValue(index);
                                                property.SetValue(instance, TypeConvertHelper.ConvertType(value, property.PropertyType));
                                            }
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
                    if (close && conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
        }

        public static int Execute(this IDbConnection conn, string sql, object parameters = null, bool close = true)
        {
            using (var command = Prepare(conn, sql, parameters))
            {
                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    var result = command.ExecuteNonQuery();
                    return result;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (close && conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
        }
    }
}