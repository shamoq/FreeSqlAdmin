using Simple.Utils.Models;

namespace Simple.Interfaces.Dtos;

public class OrgEntityDto: InputId
{
    /// <summary>名称</summary>
    public string Name { get; set; }

    /// <summary>编码</summary>
    public string Code { get; set; }

    /// <summary>全名称</summary>
    public string FullName { get; set; }

    /// <summary>
    /// 组织类型，1 公司 2 部门
    /// </summary>
    public int OrgType { get; set; }

    /// <summary>
    /// 上级组织
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