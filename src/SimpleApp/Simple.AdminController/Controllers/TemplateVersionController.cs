using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.OssService;
using Simple.AdminApplication.Enums;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Exceptions;

namespace Simple.AdminController.Controllers
{
    public class TemplateVersionController : AppAuthController
    {
        private readonly TemplateVersionService _templateVersionService;
        private LocalOssService _localOssService;

        public TemplateVersionController(TemplateVersionService richTextDataService, LocalOssService localOssService)
        {
            _templateVersionService = richTextDataService;
            this._localOssService = localOssService;
        }

        [HttpPost]
        public async Task<ApiResult> GetUmoContent(ParameterModel input)
        {
            if (input.Id == null)
            {
                return ApiResult.Fail("参数错误");
            }

            var type = input.GetAttributeValue<string>("type");
            if (string.IsNullOrEmpty(type))
            {
                type = "html";
            }

            var data = await _templateVersionService.GetUmoContent(input.Id.Value, type);
            if (type == "json" && !string.IsNullOrEmpty(data))
            {
                return ApiResult.Success(JsonConvert.DeserializeObject(data));
            }

            return ApiResult.Success(data);
        }

        [HttpPost]
        public async Task<ApiResult> SaveUmo(ParameterModel input)
        {
            if (input.Id == null)
            {
                return ApiResult.Fail("参数错误");
            }

            var html = input.GetAttributeValue<string>("html");
            var json = input.GetAttributeValue("json");
            var text = input.GetAttributeValue<string>("text");
            var data = await _templateVersionService.SaveUmoContent(input.Id.Value, html,
                JsonConvert.SerializeObject(json), text);
            return ApiResult.Success(data);
        }

        /// <summary>
        /// 合同上传word线下模板
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> UploadOfficeTemplate(ParameterModel input)
        {
            var id = input.GetAttributeValue<Guid>("id");
            var documentId = input.GetAttributeValue<Guid>("documentId");
            var category = input.GetAttributeValue<string>("category");
            await _templateVersionService.UploadOfficeTemplate(id, category == TemplateCategoryEnum.CustomTemplate ? TemplateCategoryEnum.CustomTemplate :
                TemplateCategoryEnum.ContractTemplate, documentId);
            return ApiResult.Success();
        }

        /// <summary>
        /// 获取线下word模板信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> GetOfficeTemplate(ParameterModel input)
        {
            if (input.Id == null)
            {
                return ApiResult.Fail("参数错误");
            }

            var data = await _templateVersionService.GetLatest(input.Id.Value);
            if (data == null)
            {
                return ApiResult.Success(new { });
            }

            // 如果文件没有值，则默认使用空白模板
            Guid? fileId = ValueHelper.IsNullOrEmpty(data?.FileId) ? null : data.FileId;
            return ApiResult.Success(new
            {
                data?.Id,
                fileId,
                data?.FileName,
                data?.PdfFileId,
            });
        }

        /// <summary>
        /// 预览pdf，需要将域替换成字符串信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PreviewTemplateDocx([FromQuery] ParameterModel input)
        {
            if (input.Id == null)
            {
                throw new CustomException("id参数错误");
            }

            using (var ms = await _templateVersionService.GetWordStream(input.Id.Value, null))
            {
                var bytes = StreamHelper.ToByteArray(ms);
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
        }

        /// <summary>
        /// 预览，需要将域替换成字符串信息，携带数据
        /// 如果传递了文档id(目前是合同主键)，则直接上传到文档中心，后续通过文档id进行下载预览
        /// 如果没有传递文档id，则直接返回文件流
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PreviewTemplateDataDocx(ParameterModel input)
        {
            var (templateId, data, documentId, name) =
                input.GetAttributeValue<Guid, string, Guid?, string>("templateId", "data", "documentId", "name");

            if (ValueHelper.IsNullOrEmpty(templateId))
            {
                return Content("参数错误");
            }

            var dataDic = JsonConvertHelper.ToDictionary(data);

            using (var ms = await _templateVersionService.GetWordStream(templateId, dataDic))
            {
                // 没有传递文档信息，直接返回生成的文件流
                if (documentId == null || string.IsNullOrEmpty(name))
                {
                    var bytes = StreamHelper.ToByteArray(ms);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                }
                else
                {
                    // 传递了文档信息，则同步上传文档
                    var fileName = PathHelper.GetRightFileName(name) + ".docx";
                    var document = await _localOssService.PutObjectAsync("upload", documentId.Value, fileName, ms);
                    var result = ApiResult.Success(document.Id);
                    return Json(result);
                }
            }
        }
    }
}

// /// <summary>
// /// 预览模板文件，如果已经有
// /// </summary>
// /// <param name="input"></param>
// /// <returns></returns>
// [HttpGet]
// public async Task<IActionResult> PreviewTemplate([FromQuery] Guid id, [FromQuery] string type)
// {
//     if (id == Guid.Empty)
//     {
//         return Content($"模版Id为空，参数错误");
//     }
//
//     var data = await _templateVersionService.GetLatest(id);
//
//     if (type == "pdf")
//     {
//         // 如果文件没有值，则默认使用空白模板
//         var fileId = data?.PdfFileId;
//         if (fileId != null)
//         {
//             var document = await localOssService.GetObjectAsync(fileId.Value);
//             return PhysicalFile(document.FullPath, "application/pdf");
//         }
//         else
//         {
//             // 使用word换pdf
//             var document = await localOssService.GetObjectAsync(AppConsts.EmptyDocumentId);
//             // 将文件转为pdf
//             var pdfBytes = AsposeWordHelper.Word2Pdf(document.GetFileStream());
//
//             // 返回文件内容
//             return File(pdfBytes, "application/pdf");
//         }
//     }
//     else
//     {
//         // word流
//         var fileId = data?.FileId ?? AppConsts.EmptyDocumentId;
//         var document = await localOssService.GetObjectAsync(fileId);
//         return PhysicalFile(document.FullPath,
//             "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
//     }
// }

// [HttpGet]
// public async Task<IActionResult> PreviewPdf([FromQuery] ParameterModel input)
// {
//     var businessId = input.Id;
//     if (businessId == null)
//     {
//         return Content($"businessId为空，参数错误");
//     }
//
//     var data = await _templateVersionService.GetLatest(businessId.Value);
//     // 如果文件没有值，则默认使用空白模板
//     var fileId = data?.PdfFileId;
//     if (fileId != null)
//     {
//         var document = await localOssService.GetObjectAsync(fileId.Value);
//         return PhysicalFile(document.FullPath, "application/pdf");
//     }
//     else
//     {
//         // 使用word换pdf
//         var document = await localOssService.GetObjectAsync(AppConsts.EmptyDocumentId);
//         // 将文件转为pdf
//         var pdfBytes = AsposeWordHelper.Word2Pdf(document.GetFileStream());
//
//         // 返回文件内容
//         return File(pdfBytes, "application/pdf");
//     }
// }
//
// [HttpPost]
// public async Task<IActionResult> RenderDataPdf([FromForm] RenderDataPdfInput input)
// {
//     if (ValueHelper.IsNullOrEmpty(input.BusinessId) || ValueHelper.IsNullOrEmpty(input.TemplateId))
//     {
//         return Content("参数错误");
//     }
//
//     var dataDic = JsonConvertHelper.ToDictionary(input.Data);
//     // 没有数据，则直接显示模板
//     if (dataDic == null || dataDic.Keys.Count == 0)
//     {
//         var templateVersion = await _templateVersionService.GetLatest(input.TemplateId);
//         if (templateVersion?.PdfFileId == null)
//         {
//             return Content("模板不存在");
//         }
//
//         var fullPath = localOssService.GetFullPath(templateVersion.FilePath);
//         var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
//         return File(bytes, "application/pdf");
//     }
//     else
//     {
//         var ms = await _templateVersionService.CreatePdfFileWithData(input.TemplateId, dataDic);
//         return File(ms, "application/pdf");
//     }
// }

// [HttpPost]
// public async Task<ApiResult> GetDataHtml(ParameterModel input)
// {
//     var businessId = input.GetAttributeValue<Guid?>("businessId");
//     if (businessId == null)
//     {
//         return ApiResult.Fail("id参数错误");
//     }
//
//     var json = input.GetAttributeValue<string>("json");
//     var html = input.GetAttributeValue<string>("html");
//     if (string.IsNullOrEmpty(json))
//     {
//         return ApiResult.Fail("数据参数错误");
//     }
//
//     Dictionary<string, object> data;
//     try
//     {
//         data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
//     }
//     catch (Exception)
//     {
//         return ApiResult.Fail("数据格式错误");
//     }
//
//     if (data == null)
//     {
//         return ApiResult.Fail("未读取到数据");
//     }
//
//     if (string.IsNullOrEmpty(html))
//     {
//         var templateVersion = await _templateVersionService.GetLatest(businessId.Value);
//         var resultHtml = await _templateVersionService.RenderUmo(templateVersion, "html", data);
//         return ApiResult.Success(resultHtml);
//     }
//     else
//     {
//         var resultHtml = RichTextHelper.Render(html, data);
//         return ApiResult.Success(resultHtml);
//     }
// }