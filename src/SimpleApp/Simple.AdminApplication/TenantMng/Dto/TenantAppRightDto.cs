using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.TenantMng.Dto;

public class TenantAppRightDto : BaseDto
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 应用编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public List<TenantAppRightDto> Children { get; set; }
    
    /// <summary>
    /// 动作点
    /// </summary>
    public List<TenantAppRightDto> Actions { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TenantAppRightDto dto &&
               Code == dto.Code &&
               Name == dto.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Name);
    }
}