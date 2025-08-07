using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.OssService;
using Simple.AdminApplication.Application.Wps;
using Simple.AdminApplication.Application.Wps.Dto;
using Simple.AdminApplication.Application.Wps.Enums;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AdminApplication.Helpers;
using Simple.AspNetCore.Controllers;
using Simple.Interfaces;
using Simple.Utils.Models;
using Simple.Utils.Models.Dto;

namespace Simple.AdminController.Controllers;

[Route("api/[controller]")]
public class WpsController : AppAuthController
{
    private WpsServerService _wpsServerService;
    private WpsCallBackService _wpsCallBackService;

    public WpsController(WpsServerService wpsServerService, WpsCallBackService wpsCallBackService)
    {
        _wpsServerService = wpsServerService;
        _wpsCallBackService = wpsCallBackService;
    }

    /// <summary>
    /// 获取WPS加载信息
    /// 初始化Wps编辑器，并且设置哪些参数回传给wps
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("GetLoadInfo")]
    public async Task<ApiResult> GetLoadInfo(WpsLoadInput input)
    {
        var result = await _wpsServerService.GetLoadInfo(input);
        return ApiResult.Success(result);
    }

    /// <summary>
    /// wps回调接口，获取文档的描述信息，用来后续文档加载
    /// </summary>
    /// <returns></returns>
    [HttpGet("v3/3rd/files/{fileId}")]
    public async Task<ApiResult> Info([FromHeader] WpsCallbackInput input)
    {
        var paramDto = input.Resolve();
        var info = await _wpsCallBackService.Info(paramDto);
        return ApiResult.Success(info);
    }

    /// <summary>
    /// wps回调接口，根据描述信息（Info），获取文件下载地址
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("v3/3rd/files/{fileId}/download")]
    public async Task<ApiResult> Download([FromHeader] WpsCallbackInput input)
    {
        var result = await _wpsCallBackService.Download(input);
        return ApiResult.Success(result);
    }

    /// <summary>
    /// wps回调接口，根据Wps的Download Url地址，解析文件
    /// </summary>
    /// <param name="input">文件获取输入参数</param>
    /// <returns>文件内容</returns>
    [HttpGet("DownloadFile")]
    public async Task<IActionResult> DownloadFile([FromQuery] WpsDownloadFileInput input)
    {
        // WpsDownloadFileInput为Download方法构造Url手工写的参数，所以Id是完整的GUID
        var result = await _wpsCallBackService.DownloadFile(input);
        if (result == null || !System.IO.File.Exists(result.FullPath))
        {
            return NotFound();
        }

        // 返回文件内容
        return PhysicalFile(result.FullPath, $"application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }

    /// <summary>
    /// wps回调接口，编辑过程中自动或者手动保存，都会进行文件上传
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpPut("UploadFile/{fileId}/{type}")]
    public async Task<ApiResult> UploadFile(string fileId, string type, [FromHeader] WpsCallbackInput input)
    {
        var documentId = GuidHelper.ConvertToGuid(fileId);
        using (var memoryStream = new MemoryStream())
        {
            await Request.Body.CopyToAsync(memoryStream);

            await _wpsCallBackService.UploadFile(memoryStream, documentId);
        }

        return ApiResult.Success();
    }

    /// <summary>
    /// wps回调接口，获取文档权限，用来控制是否可编辑，可打印等
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("v3/3rd/files/{fileId}/permission")]
    public async Task<ApiResult> Permission([FromHeader] WpsCallbackInput input)
    {
        var paramDto = input.Resolve();
        var info = await _wpsCallBackService.Permission(paramDto);
        return ApiResult.Success(info);
    }

    /// <summary>
    /// wps回调接口，获取用户列表，用来加载用户上下文，如显示编辑人，评论人等等
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>

    [HttpGet("v3/3rd/users")]
    public async Task<ApiResult> GetUsers([FromQuery(Name = "user_ids")] List<string> userIds)
    {
        var userGuids = userIds.Select(x => GuidHelper.ConvertToGuid(x)).ToList();
        var info = await _wpsCallBackService.GetUsers(userGuids);
        return ApiResult.Success(info);
    }

    /// <summary>
    /// wps回调接口，上传前准备，返回文件摘要算法
    /// </summary>
    /// <returns></returns>
    [HttpGet("v3/3rd/files/{fileId}/upload/prepare")]
    public async Task<ApiResult> UploadPrepare()
    {
        var result = await _wpsCallBackService.UploadPrepare();
        return ApiResult.Success(result);
    }

    /// <summary>
    /// Wps回调接口，获取文件上传地址
    /// </summary>
    /// <param name="input"></param>
    /// <param name="callbackInput"></param>
    /// <returns></returns>
    [HttpPost("v3/3rd/files/{fileId}/upload/address")]
    public async Task<ApiResult> UploadAddress(WpsUploadAddressInput input, [FromHeader] WpsCallbackInput callbackInput)
    {
        var result = await _wpsCallBackService.UploadAddress(input, callbackInput);
        return ApiResult.Success(result);
    }

    /// <summary>
    /// wps回调接口，上传完成
    /// </summary>
    /// <returns></returns>
    [HttpPost("v3/3rd/files/{fileId}/upload/complete")]
    public async Task<ApiResult> UploadComplete(WpsUploadCompeleteInput input,
        [FromHeader] WpsCallbackInput callbackInput)
    {
        var paramDto = callbackInput.Resolve();

        var dto = await _wpsCallBackService.UploadComplete(input, paramDto);
        return ApiResult.Success(dto);
    }
}