namespace Simple.AdminApplication.TenantMng.Entities
{
    /// <summary>
    /// 租户套餐表
    /// </summary>
    public class TenantPackage : DefaultNoTenantEntity
    {
        /// <summary>
        /// 套餐名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 用户数量
        /// </summary>
        public int UserCount { get; set; }
    }
}