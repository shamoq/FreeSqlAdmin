namespace Simple.AdminApplication.SysMng.Entities
{
    /// <summary>字典类型</summary>
    public class SysDicType : DefaultTenantEntity
    {
        /// <summary>名称</summary>
        public string Name { get; set; }

        /// <summary>备注</summary>
        public string Remark { get; set; }

        public int OrderId { get; set; } = 0;
    }
}