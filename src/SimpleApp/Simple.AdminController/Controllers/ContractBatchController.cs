using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminController.Controllers;

/// <summary>
/// 合同组服务
/// </summary>
public class ContractBatchController : AppAuthController
{
    private readonly ContractBatchService _contractBatchService;

    public ContractBatchController(ContractBatchService contractBatchService)
    {
        _contractBatchService = contractBatchService;
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel([FromQuery] Guid id)
    {
        var (document, bytes) = await _contractBatchService.ExportExcel(id);

        string fileExtension = Path.GetExtension(document.Name);
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileExtension, out string contentType))
        {
            // 如果找不到对应的 MIME 类型，使用默认的下载类型
            contentType = "application/octet-stream";
        }

        // 返回文件内容
        return File(bytes, contentType, document.Name);
    }

    /// <summary>
    /// 上传word模版
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> ImportExcel(ParameterModel input)
    {
        var (id, templateId, documentId) = input.GetAttributeValue<Guid, Guid, Guid>("id", "templateId", "documentId");
        var data = await _contractBatchService.ImportExcel(id, templateId, documentId);
        return ApiResult.Success(data);
    }

    /// <summary>
    /// 获取合同签署方信息
    /// </summary>
    /// <param name="pageRequest"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> GetSignParties(QueryRequestInput pageRequest)
    {
        var (total, data) = await _contractBatchService.GetSignParties(pageRequest.GetAttributeValue<Guid>("contractId"),
            pageRequest.Page, pageRequest.PageSize);
        return ApiResult.Success(new { total, data });
    }
}