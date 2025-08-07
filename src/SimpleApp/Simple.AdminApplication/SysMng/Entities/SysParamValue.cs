using Simple.AdminApplication.SysMng.Enums;

namespace Simple.AdminApplication.SysMng.Entities
{
    public class SysParamValue : DefaultTenantEntity
    {
        /// <summary>
        /// 作用域Id，组织
        /// </summary>
        public Guid? ScopeId { get; set; }

        /// <summary>
        /// 参数编码
        /// </summary>
        public string ParamCode { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [EnumColumn(typeof(ParamTypeEnum), "ParamTypeText")]
        public int ParamType { get; set; }
        
        /// <summary>
        /// 参数类型
        /// </summary>
        public string ParamTypeText { get; set; }
    }
}