using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.SysMng.Dto
{
    /// <summary>
    /// 参数左侧导航
    /// </summary>
    public class ParamNavDto
    {
        /// <summary>
        /// 参数Id
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// 导航标题
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 分组描述
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// 参数组配置信息
    /// </summary>
    public class ParamGroupDto
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分组描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 组参数列表
        /// </summary>
        public List<ParamItemDto> Items { get; set; }
    }
    
    /// <summary>
    /// 具体参数Dto
    /// </summary>
    public class ParamItemDto : BaseDto
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数编码
        /// </summary>
        public string ParamCode { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHide { get; set; }

        /// <summary>
        /// 权限信息
        /// </summary>
        public string FunctionCode { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public string ParamTypeText { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public int ParamType { get; set; }

        /// <summary>
        /// 选项数据源
        /// </summary>
        public List<OptionObjectDto> Options { get; set; }
    }
}