using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;

namespace Simple.AdminApplication.TenantMng.Helper;

public class MetadataHelper
{
    /// <summary>
    /// 获取应用权限
    /// </summary>
    /// <returns></returns>
    public static List<TenantAppRightDto> GetTenantAppRights()
    {
        List<TenantAppRightDto> result = new List<TenantAppRightDto>();
        // 读取 metadata/MyFunction下的所有json文件，合并到一个集合中
        var path = PathHelper.Combine(AppContext.BaseDirectory, "metadata", "MyFunction");
        var files = Directory.GetFiles(path, "*.json");
        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var appRight = JsonConvert.DeserializeObject<TenantAppRightDto>(json);
            result.Add(appRight); 
        }
        return result.OrderBy(p => p.Order).ToList();
    }
}