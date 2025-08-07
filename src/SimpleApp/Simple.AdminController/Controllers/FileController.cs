using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.OssService;
using Simple.AdminApplication.Entitys;
using Simple.AdminApplication.Helpers;
using Simple.AdminController.Models;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminController.Controllers;

[Route("api/[controller]/[action]")]
public class FileController : AppAuthController
{
    private LocalOssService localOssService;

    public FileController(LocalOssService localOssService)
    {
        this.localOssService = localOssService;
    }

    /// <summary>
    /// 获取文件
    /// </summary>
    /// <returns>文件内容</returns>
    [HttpGet]
    public async Task<IActionResult> Download([FromQuery] Guid id, [FromQuery] string token,
        [FromQuery] string fileName = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            return NotFound();
        }

        var document = await localOssService.GetObjectAsync(id);

        string fileExtension = Path.GetExtension(document.Name);
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileExtension, out string contentType))
        {
            // 如果找不到对应的 MIME 类型，使用默认的下载类型
            contentType = "application/octet-stream";
        }

        // 如果未传入文件名，则使用文档原始名称
        var downloadName = PathHelper.GetRightFileName(fileName ??= document.Name);

        // 返回文件内容
        return PhysicalFile(document.FullPath, contentType, downloadName);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="file">文件内容</param>
    /// <returns>操作结果</returns>
    [HttpPost]
    public async Task<ApiResult> Upload([FromForm] string fileName, [FromForm] IFormFile file,
        [FromForm] string bucketName)
    {
        if (file == null || file.Length == 0)
        {
            return ApiResult.Fail("文件不能为空");
        }

        var document =
            await localOssService.PutObjectAsync(bucketName ?? "upload", null, fileName, file.OpenReadStream());
        return ApiResult.Success(document.Id);
    }

    /// <summary>
    /// 获取文件[仅用于测试，实际使用时需要根据实际情况进行修改]
    /// </summary>
    /// <returns>文件内容</returns>
    [HttpGet]
    public async Task<IActionResult> Word2Pdf([FromQuery] Guid id, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return NotFound();
        }

        var document = await localOssService.GetObjectAsync(id);
        // 将文件转为pdf
        var pdfBytes = AsposeWordHelper.Word2Pdf(document.GetFileStream());

        // 返回文件内容
        return File(pdfBytes, "application/pdf");
    }


    // /// <summary>
    // /// 下载URL文件
    // /// </summary>
    // /// <param name="url">文件URL</param>
    // /// <returns>文件流</returns>
    // [HttpGet]
    // [AllowAnonymous]
    // public async Task<IActionResult> DownloadFromUrl([FromQuery] string url)
    // {
    //     if (string.IsNullOrEmpty(url))
    //     {
    //         return BadRequest("URL不能为空");
    //     }
    //
    //     using var httpClient = new HttpClient();
    //     var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    //
    //     if (!response.IsSuccessStatusCode)
    //     {
    //         return StatusCode((int)response.StatusCode, "文件下载失败");
    //     }
    //
    //     // 获取文件名
    //     var fileName = response.Content.Headers.ContentDisposition?.FileName 
    //                    ?? Path.GetFileName(new Uri(url).AbsolutePath);
    //
    //     // 获取文件类型
    //     var contentType = response.Content.Headers.ContentType?.MediaType;
    //     if (string.IsNullOrEmpty(contentType))
    //     {
    //         // 根据文件后缀判断类型
    //         var fileExtension = Path.GetExtension(fileName);
    //         var provider = new FileExtensionContentTypeProvider();
    //         if (!provider.TryGetContentType(fileExtension, out contentType))
    //         {
    //             // 如果还是找不到对应的 MIME 类型，使用默认的下载类型
    //             contentType = "application/octet-stream";
    //         }
    //     }
    //
    //     // 返回文件流
    //     var fileStream = await response.Content.ReadAsStreamAsync();
    //     return File(fileStream, contentType, fileName);
    // }
}