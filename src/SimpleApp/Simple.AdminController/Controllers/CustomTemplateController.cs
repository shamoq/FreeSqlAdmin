using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AdminApplication.Enums;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Exceptions;

namespace Simple.AdminController.Controllers;

public class CustomTemplateController : AppCurdController<CustomTemplate, CustomTemplateDto>
{
    private CustomTemplateService _service;

    public CustomTemplateController(CustomTemplateService service)
    {
        _service = service;
    }

    public override async Task<ApiResult> Page(QueryRequestInput pageRequest)
    {
        var (total, list) = await Service.Page(pageRequest);
        var dtoList = list.Adapt<List<CustomTemplateDto>>();

        foreach (var dto in dtoList)
        {
            dto.SetAttributeValue("templateTypeText", StaticClassEnumHelper.GetDescription(dto.TemplateType, typeof(TemplateTypeEnum)));
        }

        return ApiResult.Success(new { total, data = dtoList });
    }

    [HttpPost]
    public async Task<ApiResult> GetDetail(ParameterModel input)
    {
        if (input.Id == null)
        {
            return ApiResult.Fail("参数错误");
        }

        var template = await _service.GetDetail(input.Id.Value);
        return ApiResult.Success(template);
    }

    [HttpPost]
    public async Task<ApiResult> SaveDetail(CustomTemplateDto dto)
    {
        var template = await _service.SaveDetail(dto);
        return ApiResult.Success(template);
    }

    /// <summary>
    /// 预览pdf，需要将域替换成字符串信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> PreviewDocx([FromQuery] Guid id, [FromQuery] int index)
    {
        if (id == Guid.Empty)
        {
            throw new CustomException("id参数错误");
        }

        using (var ms = await _service.GetWordStream(id, index))
        {
            var bytes = StreamHelper.ToByteArray(ms);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        }
    }

    /// <summary>
    /// 预览pdf，需要将域替换成字符串信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> DownloadAll([FromQuery] Guid id, [FromQuery] int? isMerege, [FromQuery] string fileName = null)
    {
        if (id == Guid.Empty)
        {
            throw new CustomException("id参数错误");
        }

        // 如果未传入文件名，则使用文档原始名称
        var downloadName = PathHelper.GetRightFileName(fileName);
        var contentType = string.Empty;
        if (isMerege == 1)
        {
            downloadName += ".docx";
            contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        }
        else
        {
            downloadName += ".zip";
            contentType = "application/zip";
        }

        using (var ms = await _service.GetAllWordStream(id, isMerege == 1))
        {
            var bytes = StreamHelper.ToByteArray(ms);
            return File(bytes, contentType, downloadName);
        }
    }
}