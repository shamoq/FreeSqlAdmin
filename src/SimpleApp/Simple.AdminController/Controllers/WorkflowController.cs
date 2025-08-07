using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.Workflows;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Dtos.Bpm;
using Simple.AdminApplication.Enums;
using Simple.AspNetCore.Controllers;
using Simple.AspNetCore.Services;

namespace Simple.AdminController.Controllers;

/// <summary>
/// 流程控制器
/// </summary>
public class WorkflowController : AppAuthController
{
    private BpmInstanceService _bpmInstanceService;
    private BpmTemplateStepService _bpmTemplateStepService;
    private UserContextService _userContextService;
    private ContractService _contractService;

    public WorkflowController(BpmInstanceService bpmInstanceService, BpmTemplateStepService bpmTemplateStepService,
        ContractService contractService, UserContextService userContextService)
    {
        _bpmInstanceService = bpmInstanceService;
        _bpmTemplateStepService = bpmTemplateStepService;
        _contractService = contractService;
        _userContextService = userContextService;
    }

    [HttpPost]
    public async Task<ApiResult> CheckBeforeLanch(BpmApproveInput input)
    {
        // 目前只有合同这个业务，所以直接写死，如果有扩展，可以抽象成接口，根据类型获取实现
        await _contractService.CheckBeforeLanch(input.BusinessId);
        return ApiResult.Success();
    }

    /// <summary>
    /// 撤回
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> Recall(BpmApproveInput input)
    {
        await _bpmInstanceService.Recall(input);
        return ApiResult.Success();
    }

    /// <summary>
    /// 审批
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> Approve(BpmApproveInput input)
    {
        if (input.BusinessId == Guid.Empty)
        {
            return ApiResult.Fail("业务Id不能为空");
        }

        if (input.StepResult > StepResultEnum.Lanch &&
            (input.InstanceStepId == null || input.InstanceStepId == Guid.Empty))
        {
            return ApiResult.Fail("审批步骤Id不能为空");
        }

        if (input.StepResult == (int)StepResultEnum.Reject)
        {
            if (string.IsNullOrEmpty(input.Remark))
            {
                return ApiResult.Fail("驳回原因不能为空");
            }
        }

        var user = _userContextService.GetUser();
        input.UserId = user.Id;
        input.UserName = user.UserName;

        if (input.StepResult == (int)StepResultEnum.Lanch)
        {
            await _contractService.CheckBeforeLanch(input.BusinessId);
            if (input.InstanceStepId == null) // 首次发起
            {
                await _bpmInstanceService.CreateApprove(input);
            }
            else // 重新发起
            {
                await _bpmInstanceService.Approve(input);
            }
        }
        else if (input.StepResult == StepResultEnum.Pass)
        {
            await _bpmInstanceService.Approve(input);  
        }
        else if (input.StepResult == StepResultEnum.Reject)
        {
            await _bpmInstanceService.Reject(input);
        }
        else if (input.StepResult == StepResultEnum.Abort)
        {
            await _contractService.CheckBeforeAbort(input.BusinessId);
            await _bpmInstanceService.Abort(input);
        }

        return ApiResult.Success();
    }

    /// <summary>
    /// 获取审批步骤
    /// </summary>
    [HttpPost]
    public async Task<ApiResult> GetApproveStep(ParameterModel input)
    {
        var templateId = input.GetAttributeValue<Guid?>("templateId");
        if (templateId == null || input.Id == null)
        {
            return ApiResult.Fail("参数错误！");
        }

        // 有实例获取流程实例
        var instance = await _bpmInstanceService.FindById(input.Id.Value);
        if (instance != null)
        {
            var dto = await _bpmInstanceService.GetBusinessSteps(input.Id.Value, instance);
            return ApiResult.Success(dto);
        }
        else
        {
            var dto = await _bpmTemplateStepService.GetBusinessSteps(templateId.Value);
            return ApiResult.Success(dto);
        }
    }

    /// <summary>
    /// 获取审批信息
    /// </summary>
    [HttpPost]
    public async Task<ApiResult> GetApproveInfo(ParameterModel input)
    {
        var businessId = input.GetAttributeValue<Guid?>("businessId");
        if (businessId == null)
        {
            return ApiResult.Fail("参数错误！");
        }
        var approveInfo = await _bpmInstanceService.GetApproveInfo(businessId.Value);
        return ApiResult.Success(approveInfo);
    }
}