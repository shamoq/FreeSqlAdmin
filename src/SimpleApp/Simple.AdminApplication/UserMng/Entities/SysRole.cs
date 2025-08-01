namespace Simple.AdminApplication.UserMng.Entities
{
    /// <summary>
    /// 角色
    /// </summary>
    public class SysRole : DefaultTenantEntity, ITreeEntity
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色全名称
        /// </summary>
        public string FullName { get; set; }
        
        /// <summary>
        /// 排序编码
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 排序编码
        /// </summary>
        public string OrderFullCode { get; set; }
        
        /// <summary>
        /// 父级Id
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}