namespace Simple.Utils.Generator.Dto
{
    /// <summary>
    /// 生成的数据库sql
    /// </summary>
    public class GenerateDatabaseSqlResult
    {
        /// <summary>
        /// 创建表的语句
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 字段变更语句
        /// </summary>
        public List<string> FieldModifySqls { get; set; } = new List<string>();
    }
}