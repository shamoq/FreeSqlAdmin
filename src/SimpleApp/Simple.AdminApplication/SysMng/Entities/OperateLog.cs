namespace Simple.AdminApplication.SysMng.Entities
{
    /// <summary>
    /// 操作日志
    /// </summary>
    public class OperateLog : DefaultTenantEntity
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperateType { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string Description { get; set; }
    }
}