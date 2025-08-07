namespace Simple.Utils.Helper;

public class SqlValidatorHelper
{
    // 禁止的 SQL 关键字（DDL/DML 命令）
    private static readonly string[] ProhibitedKeywords = new[]
    {
        "ALTER", "CREATE", "DROP", "TRUNCATE", "INSERT", "UPDATE", "DELETE", 
        "EXEC", "EXECUTE", "MERGE", "GRANT", "REVOKE", "COMMIT", "ROLLBACK", "BEGIN", "END"
    };

    /// <summary>
    /// 校验 SQL 是否为纯查询语句（仅包含 SELECT）
    /// </summary>
    public static bool IsSafeSelectQuery(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        // 移除注释和多余空格
        var cleanedSql = RemoveCommentsAndTrim(sql);
        
        // 检查是否以 SELECT 开头
        if (!cleanedSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            return false;
            
        // 检查是否包含禁止的关键字
        foreach (var keyword in ProhibitedKeywords)
        {
            // 使用正则表达式匹配完整的单词
            var pattern = @$"\b{keyword}\b";
            if (System.Text.RegularExpressions.Regex.IsMatch(cleanedSql, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 移除 SQL 中的注释并修剪空格
    /// </summary>
    private static string RemoveCommentsAndTrim(string sql)
    {
        // 移除单行注释
        var result = System.Text.RegularExpressions.Regex.Replace(
            sql, @"--.*$", string.Empty, System.Text.RegularExpressions.RegexOptions.Multiline);
            
        // 移除多行注释
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"/\*.*?\*/", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);
            
        // 压缩空格
        return System.Text.RegularExpressions.Regex.Replace(
            result, @"\s+", " ", System.Text.RegularExpressions.RegexOptions.Singleline).Trim();
    }
}