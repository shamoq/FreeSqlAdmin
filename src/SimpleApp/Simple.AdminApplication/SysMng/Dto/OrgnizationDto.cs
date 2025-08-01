using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.SysMng.Dto;

public class OrgnizationDto : BaseDto, ITreeDto
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>名称</summary>
    public string Name { get; set; }

    /// <summary>编码</summary>
    public string Code { get; set; }

    /// <summary>全名称</summary>
    public string FullName { get; set; }

    /// <summary>负责人</summary>
    public string Contract { get; set; }

    /// <summary>
    /// 组织类型，1 公司 2 部门
    /// </summary>
    public int OrgType { get; set; }

    /// <summary>
    /// 上级组织
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 所属公司
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>备注</summary>
    public string Remark { get; set; }

    /// <summary>是否公司</summary>
    public bool IsCompany { get; set; }

    /// <summary>
    /// 是否末级公司
    /// </summary>
    public bool IsEndCompany { get; set; }

    /// <summary>
    /// 是否部门
    /// </summary>
    public bool IsDepartment { get; set; }

    /// <summary>
    /// 是否末级部门
    /// </summary>
    public bool IsEndDepartment { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnable { get; set; }
}