namespace Simple.AdminApplication.TenantMng.Entities
{
    public class Tenant : DefaultNoTenantEntity
    {
        /// <summary>
        /// 租户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 租户编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 租户类型（正式，测试）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 租户描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 租户状态
        /// </summary>
        public int IsEnable { get; set; } = 1;

        /// <summary>
        /// 租户Logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 数据库链接
        /// </summary>
        public Guid DataSourceId { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpirationTime { get; set; }
        
        /// <summary>
        /// 租户套餐Id
        /// </summary>
        public Guid TenantPackageId { get; set; }
    }
}