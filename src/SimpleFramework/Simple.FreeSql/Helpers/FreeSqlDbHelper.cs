using System.Data;
using System.Text.RegularExpressions;
using FreeSql;
using FreeSql.Internal.Model;
using Simple.Utils.Helper;

namespace Simple.AdminApplication.Helpers;

public class FreeSqlDbHelper
{
    public static string EnsureConnectionTimeout(string connectionString, DataType dataType, int timeoutSeconds = 1)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        string timeoutParam = string.Empty;
        bool hasTimeout = false;

        // 根据数据库类型设置超时参数
        switch (dataType)
        {
            case DataType.MySql:
                timeoutParam = $"Connect Timeout={timeoutSeconds}";
                hasTimeout = Regex.IsMatch(connectionString, @"Connect Timeout\s*=\s*\d+|Connection Timeout\s*=\s*\d+|connectTimeout\s*=\s*\d+", RegexOptions.IgnoreCase);
                break;
                
            case DataType.SqlServer:
                timeoutParam = $"Connect Timeout={timeoutSeconds}";
                hasTimeout = Regex.IsMatch(connectionString, @"Connect Timeout\s*=\s*\d+", RegexOptions.IgnoreCase);
                break;
                
            case DataType.PostgreSQL:
                timeoutParam = $"connect_timeout={timeoutSeconds}";
                hasTimeout = Regex.IsMatch(connectionString, @"connect_timeout\s*=\s*\d+", RegexOptions.IgnoreCase);
                break;
                
            case DataType.Oracle:
                timeoutParam = $"Connection Timeout={timeoutSeconds}";
                hasTimeout = Regex.IsMatch(connectionString, @"Connection Timeout\s*=\s*\d+", RegexOptions.IgnoreCase);
                break;
                
            case DataType.Sqlite:
                return connectionString; // SQLite没有专门的连接超时参数
                
            case DataType.Dameng:
                timeoutParam = $"Connection Timeout={timeoutSeconds}";
                hasTimeout = Regex.IsMatch(connectionString, @"Connection Timeout\s*=\s*\d+", RegexOptions.IgnoreCase);
                break;
                
            case DataType.GBase:
                timeoutParam = $"Connect Timeout={timeoutSeconds}";
                hasTimeout = Regex.IsMatch(connectionString, @"Connect Timeout\s*=\s*\d+", RegexOptions.IgnoreCase);
                break;
                
            case DataType.ShenTong:
                timeoutParam = $"Connect Timeout={timeoutSeconds}";
                hasTimeout = Regex.IsMatch(connectionString, @"Connect Timeout\s*=\s*\d+", RegexOptions.IgnoreCase);
                break;
                
            default:
                return connectionString; // 未知数据库类型，返回原始连接字符串
        }

        // 如果已包含超时参数，直接返回
        if (hasTimeout)
            return connectionString;

        // 根据连接字符串格式选择追加方式
        if (connectionString.Contains("?") && !connectionString.EndsWith("?"))
            return connectionString + "&" + timeoutParam;
        else
            return connectionString.TrimEnd(';') + ";" + timeoutParam + ";";
    }
    
    /// <summary>
    /// 测试连接
    /// </summary>
    /// <param name="dbType"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static async Task<bool> CanConnect(DataType dbType, string connectionString)
    {
        var timeoutConnectionString = EnsureConnectionTimeout(connectionString, dbType);
        try
        {
            using var fsql = new FreeSqlBuilder()
                .UseConnectionString(dbType, timeoutConnectionString)
                .Build();

            var result = await fsql.Ado.CommandFluent("SELECT 1")
                .CommandTimeout(1).ExecuteScalarAsync();
            return result != null;
        }
        catch (OperationCanceledException)
        {
            return false; // 超时返回false
        }
        catch
        {
            return false; // 其他异常也返回false
        }
    }

    /// <summary>
    /// 执行多条SQL
    /// </summary>
    public static async Task<List<object>> GetData(DataType dbType, string connectionString, List<string> sqls,
        List<bool> isSingles)
    {
        using (IFreeSql fsql = new FreeSqlBuilder()
                   .UseConnectionString(dbType, connectionString)
                   .Build())
        {
            var list = new List<object>();
            for (int i = 0; i < sqls.Count; i++)
            {
                var sql = sqls[i];
                var isSingle = isSingles[i];

                if (isSingle)
                {
                    var data = await fsql.Ado.QuerySingleAsync<Dictionary<string, Object>>(sql);
                    list.Add(data);
                }
                else
                {
                    var data = await fsql.Ado.QueryAsync<Dictionary<string, Object>>(sql);
                    list.Add(data);
                }
            }

            return list;
        }
    }

    /// <summary>
    /// 执行多条SQL
    /// </summary>
    public static async Task<List<DataTable>> GetDataTable(DataType dbType, string connectionString, List<string> sqls)
    {
        using (IFreeSql fsql = new FreeSqlBuilder()
                   .UseConnectionString(dbType, connectionString)
                   .Build())
        {
            var list = new List<DataTable>();
            foreach (var sql in sqls)
            {
                var data = await fsql.Ado.ExecuteDataTableAsync(sql);
                list.Add(data);
            }

            return list;
        }
    }

    /// <summary>
    /// 获取指定列名的注释信息
    /// </summary>
    /// <param name="dbType">数据库类型</param>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="columnNames">要查询的列名列表</param>
    /// <returns>列名到注释的映射字典</returns>
    public static async Task<Dictionary<string, string>> GetColumnComments(DataType dbType, string connectionString,
        List<string> columnNames)
    {
        if (columnNames == null || columnNames.Count == 0)
            return new Dictionary<string, string>();

        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(dbType, connectionString)
            .Build();

        var sql = dbType switch
        {
            DataType.MySql => @"
                SELECT COLUMN_NAME, COLUMN_COMMENT 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = DATABASE() 
                  AND COLUMN_NAME IN ({columnNames})",

            DataType.PostgreSQL => @"
                SELECT a.attname AS COLUMN_NAME, d.description AS COLUMN_COMMENT
                FROM pg_class c
                JOIN pg_attribute a ON a.attrelid = c.oid
                LEFT JOIN pg_description d ON d.objoid = c.oid AND d.objsubid = a.attnum
                WHERE c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = current_schema())
                  AND a.attname IN ({columnNames})
                  AND a.attnum > 0
                  AND NOT a.attisdropped",

            DataType.SqlServer => @"
                SELECT c.COLUMN_NAME, CAST(ISNULL(ep.value, '') AS NVARCHAR(MAX)) AS COLUMN_COMMENT
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = OBJECT_ID(c.TABLE_NAME) 
                    AND ep.minor_id = c.ORDINAL_POSITION 
                    AND ep.name = 'MS_Description'
                WHERE COLUMN_NAME IN ({columnNames})",

            DataType.Oracle => @"
                SELECT COLUMN_NAME, COMMENTS AS COLUMN_COMMENT
                FROM USER_COL_COMMENTS
                WHERE COLUMN_NAME IN ({columnNames})",

            DataType.Dameng => @"
                SELECT COLUMN_NAME, COMMENTS AS COLUMN_COMMENT
                FROM ALL_COL_COMMENTS
                WHERE OWNER = USER
                  AND COLUMN_NAME IN ({columnNames})",

            DataType.ShenTong => @"
                SELECT a.attname AS COLUMN_NAME, d.description AS COLUMN_COMMENT
                FROM pg_class c
                JOIN pg_attribute a ON a.attrelid = c.oid
                LEFT JOIN pg_description d ON d.objoid = c.oid AND d.objsubid = a.attnum
                WHERE c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = current_schema())
                  AND a.attname IN ({columnNames})
                  AND a.attnum > 0
                  AND NOT a.attisdropped",

            DataType.KingbaseES => @"
                SELECT a.attname AS COLUMN_NAME, d.description AS COLUMN_COMMENT
                FROM pg_class c
                JOIN pg_attribute a ON a.attrelid = c.oid
                LEFT JOIN pg_description d ON d.objoid = c.oid AND d.objsubid = a.attnum
                WHERE c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = current_schema())
                  AND a.attname IN ({columnNames})
                  AND a.attnum > 0
                  AND NOT a.attisdropped",

            DataType.GBase => @"
                SELECT COLUMN_NAME, COLUMN_COMMENT 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = DATABASE() 
                  AND COLUMN_NAME IN ({columnNames})",

            DataType.Xugu => @"
                SELECT COLUMN_NAME, COMMENTS AS COLUMN_COMMENT
                FROM USER_COL_COMMENTS
                WHERE COLUMN_NAME IN ({columnNames})",

            // DataType.Highgo => @"
            //     SELECT a.attname AS COLUMN_NAME, d.description AS COLUMN_COMMENT
            //     FROM pg_class c
            //     JOIN pg_attribute a ON a.attrelid = c.oid
            //     LEFT JOIN pg_description d ON d.objoid = c.oid AND d.objsubid = a.attnum
            //     WHERE c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = current_schema())
            //       AND a.attname IN ({columnNames})
            //       AND a.attnum > 0
            //       AND NOT a.attisdropped",
            //
            // DataType.Gauss => @"
            //     SELECT a.attname AS COLUMN_NAME, d.description AS COLUMN_COMMENT
            //     FROM pg_class c
            //     JOIN pg_attribute a ON a.attrelid = c.oid
            //     LEFT JOIN pg_description d ON d.objoid = c.oid AND d.objsubid = a.attnum
            //     WHERE c.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = current_schema())
            //       AND a.attname IN ({columnNames})
            //       AND a.attnum > 0
            //       AND NOT a.attisdropped",

            _ => throw new NotSupportedException($"不支持的数据库类型: {dbType}")
        };


        var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var sqlvalues = string.Join(",", columnNames.Select(name => $"'{name}'").ToList());
            sql = sql.Replace("{columnNames}", sqlvalues);
            var result = await fsql.Ado.QueryAsync<dynamic>(sql);
            foreach (Dictionary<string, object> row in result)
            {
                var key = row["COLUMN_NAME"]?.ToString();
                var comment = row["COLUMN_COMMENT"]?.ToString();
                dic[key] = comment;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取列注释时出错: {ex.Message}");
        }

        return dic;
    }
}