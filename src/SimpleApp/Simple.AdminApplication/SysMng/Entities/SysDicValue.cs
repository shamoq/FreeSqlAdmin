namespace Simple.AdminApplication.SysMng.Entities
{
    /// <summary>字典值</summary>
    public class SysDicValue : DefaultTenantEntity, ITreeEntity
    {
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>名称</summary>
        public string Name { get; set; }

        public string Value { get; set; }

        /// <summary>备注</summary>
        public string Remark { get; set; }

        public int DicTypeId { get; set; }

        /// <summary>
        /// 全名称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public int IsEnable { get; set; }

        /// <summary>
        /// 父级GUID
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 排序编码
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 排序全编码
        /// </summary>
        public string OrderFullCode { get; set; }
    }
}