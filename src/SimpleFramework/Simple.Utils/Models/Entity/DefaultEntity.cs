using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Simple.Utils.Attributes;

namespace Simple.Utils.Models.Entity
{
    /// <summary>主键为int型的基类</summary>
    [GenerateTypeScript]
    [NotMapped]
    public class DefaultEntity : DyamicJson
    {
        /// <summary>主键</summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = default;

        /// <summary>
        /// 创建人GUID
        /// </summary>
        public Guid? CreatedId { get; set; }

        /// <summary>创建时间</summary>
        public DateTime? CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建人
        /// </summary>
        [StringLength(128)]
        public string Creator { get; set; }

        /// <summary>更新时间</summary>
        public DateTime? UpdatedTime { get; set; }

        /// <summary>
        /// 更新人GUID
        /// </summary>
        public Guid? UpdatedId { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [StringLength(128)]
        public string Updator { get; set; }
    }

    /// <summary>
    /// 多租户实体
    /// </summary>
    [NotMapped]
    public abstract class DefaultTenantEntity : DefaultEntity
    {
        /// <summary>
        /// 租户GUID
        /// </summary>
        public Guid TenantId { get; set; }
    }

    /// <summary>
    /// 非多租户实体
    /// </summary>
    [NotMapped]
    public abstract class DefaultNoTenantEntity : DefaultEntity
    {
    }
}