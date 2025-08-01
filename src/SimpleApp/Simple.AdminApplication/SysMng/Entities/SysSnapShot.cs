using System.ComponentModel.DataAnnotations;

namespace Simple.AdminApplication.SysMng.Entities
{
    /// <summary>
    /// 快照服务
    /// </summary>
    public class SysSnapShot : DefaultTenantEntity
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 数据值
        /// </summary>
        [StringLength(-1)]
        public string Content { get; set; }

        /// <summary>
        /// 数据hash值
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// 业务Id
        /// </summary>
        public Guid? BusinessId { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public int Version { get; set; } = 1;
    }
}