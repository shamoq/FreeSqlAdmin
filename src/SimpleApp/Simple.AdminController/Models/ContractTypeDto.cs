using Simple.Utils.Models.Dto;

namespace Simple.AdminController.Models;

public class ContractTypeDto : BaseDto
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public int IsEnable { get; set; }

    /// <summary>
    /// 合同审批方式
    /// </summary>
    public string ApproveMode { get; set; }

    /// <summary>
    /// 线下审批人
    /// </summary>
    public string Approver { get; set; }

    /// <summary>
    /// 作废审批方式
    /// </summary>
    public string CancelApproveMode { get; set; }

    /// <summary>
    /// 作废审批人
    /// </summary>
    public string CancelApprover { get; set; }

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