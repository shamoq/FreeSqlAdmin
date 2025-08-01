using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.OssService;
using Simple.AdminApplication.Application.Wps.Entities;
using Simple.AdminApplication.DbContexts;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AdminApplication.Enums;
using Simple.AdminController.Models;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Consts;
using Simple.Utils.Exceptions;
using Simple.Utils.Models.Dto;

namespace Simple.AdminController.Controllers;

public class BzTemplateController : AppAuthController
{
    private IWebHostEnvironment _hostEnvironment;
    private TemplateVersionService _templateVersionService;
    private AdminDbContext _adminDbContext;

    public BzTemplateController(IWebHostEnvironment hostEnvironment,
        TemplateVersionService templateVersionService, AdminDbContext adminDbContext)
    {
        _hostEnvironment = hostEnvironment;
        _templateVersionService = templateVersionService;
        _adminDbContext = adminDbContext;
    }

    private async Task<BzTemplateDto> GetBzTemplate()
    {
        var root = _hostEnvironment.WebRootPath;
        var file = PathHelper.Combine(root, "files", "bztemplate", "list.json");
        if (!System.IO.File.Exists(file))
        {
            return new BzTemplateDto();
        }

        var content = await System.IO.File.ReadAllTextAsync(file);
        var dto = JsonConvert.DeserializeObject<BzTemplateDto>(content);
        return dto;
    }

    [HttpPost]
    public async Task<ApiResult> GetList()
    {
        var dto = await GetBzTemplate();
        return ApiResult.Success(dto);
    }

    [HttpPost]
    public async Task<ApiResult> Introduce(ParameterModel input)
    {
        var (type, ids) = input.GetAttributeValue<string, List<string>>("type", "ids");
        if (string.IsNullOrEmpty(type))
        {
            return ApiResult.Fail("类型不能为空");
        }

        if (ids == null || ids.Count == 0)
        {
            return ApiResult.Fail("模板ID不能为空");
        }

        var dto = await GetBzTemplate();
        var list = dto.Files.Where(t => ids.Contains(t.Id)).ToList();
        if (list.Count == 0)
        {
            return ApiResult.Fail("模板不存在");
        }

        // 插入合同模板
        if (type.Equals(TemplateCategoryEnum.ContractTemplate, StringComparison.OrdinalIgnoreCase))
        {
            var templateNames = await _adminDbContext.Template.Select(t => t.Name).ToListAsync();
            foreach (var item in list)
            {
                var newName = GetUniqueName(Path.GetFileNameWithoutExtension(item.Name), templateNames);
                var template = new Template()
                {
                    Id = Guid.NewGuid(),
                    Name = newName,
                    Remark = item.Remark,
                    TemplateType = TemplateTypeEnum.Word,
                    ContractTypeId = Guid.Empty,
                };
                await _adminDbContext.Template.AddAsync(template);

                if (string.IsNullOrEmpty(item.Files))
                {
                    throw new CustomException("范本合同文件不存在");
                }

                var fileDtos = JsonConvert.DeserializeObject<List<FileDocumentDto>>(item.Files);
                if (fileDtos == null || fileDtos.Count == 0)
                {
                    throw new CustomException("范本合同文件不存在");
                }

                //上传文件
                // var path = Path.Combine(_hostEnvironment.WebRootPath, "files", "bztemplate",
                //     item.Id + ".docx");
                await _templateVersionService.UploadOfficeTemplate(template.Id, TemplateCategoryEnum.ContractTemplate, fileDtos.First().DocumentId);
            }
        }
        else if (type.Equals(TemplateCategoryEnum.CustomTemplate, StringComparison.OrdinalIgnoreCase))
        {
            var templateNames = await _adminDbContext.CustomTemplate.Select(t => t.Name).ToListAsync();
            foreach (var item in list)
            {
                var newName = GetUniqueName(Path.GetFileNameWithoutExtension(item.Name), templateNames);
                var template = new CustomTemplate()
                {
                    Id = Guid.NewGuid(),
                    Name = newName,
                    Remark = item.Remark,
                    TemplateType = TemplateTypeEnum.Word,
                };
                await _adminDbContext.CustomTemplate.AddAsync(template);

                if (string.IsNullOrEmpty(item.Files))
                {
                    throw new CustomException("范本合同文件不存在");
                }

                var fileDtos = JsonConvert.DeserializeObject<List<FileDocumentDto>>(item.Files);
                if (fileDtos == null || fileDtos.Count == 0)
                {
                    throw new CustomException("范本合同文件不存在");
                }

                //上传文件
                // var path = Path.Combine(_hostEnvironment.WebRootPath, "files", "bztemplate",
                //     item.Id + ".docx");
                await _templateVersionService.UploadOfficeTemplate(template.Id, TemplateCategoryEnum.CustomTemplate, fileDtos.First().DocumentId);
            }
        }

        return ApiResult.Success();
    }

    private string GetUniqueName(string name, List<string> names)
    {
        var index = 1;
        var baseName = Path.GetFileNameWithoutExtension(name);
        var uniqueName = name;
        while (names.Contains(uniqueName))
        {
            uniqueName = $"{baseName}({index})";
            index++;
        }

        return uniqueName;
    }

    /// <summary>
    /// 保存
    /// </summary>
    [HttpPost]
    public async Task<ApiResult> Save(BzTemplateDto input)
    {
        var root = _hostEnvironment.WebRootPath;
        var file = PathHelper.Combine(root, "files", "bztemplate", "list.json");
        var json = JsonConvert.SerializeObject(input, Formatting.Indented);
        await System.IO.File.WriteAllTextAsync(file, json);
        return ApiResult.Success();
    }
}