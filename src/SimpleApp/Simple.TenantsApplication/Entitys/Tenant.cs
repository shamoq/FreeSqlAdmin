using System.ComponentModel.DataAnnotations.Schema;

namespace Simple.TenantsApplication.Entitys
{
    [Table("p_Tenant")]
    public class Tenant : DefaultEntity
    {
        /// <summary>
        /// 租户的名称
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// 租户的完整名称
        /// </summary>
        public string TenantFullName { get; set; }

        /// <summary>
        /// 租户的代码
        /// </summary>
        public string TenantCode { get; set; }

        /// <summary>
        /// 租户的状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 租户服务的过期时间
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 租户环境信息
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// 租户的数据库
        /// </summary>
        public string Db { get; set; }

        /// <summary>
        /// 数据库版本mysql连接专用
        /// </summary>
        public string DbVersion { get; set; }
    }
}