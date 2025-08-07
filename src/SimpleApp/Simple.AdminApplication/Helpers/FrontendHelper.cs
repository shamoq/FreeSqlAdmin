using Newtonsoft.Json;
using Simple.AdminApplication.TenantMng.Helper;

namespace Simple.AdminApplication.Helpers;

public class FrontendHelper
{
    /// <summary>
    /// 生成前端代码,Dto 枚举 实体定义
    /// </summary>
    public static void Create()
    {
        // 生成typescript定义
        var apiPath = PathHelper.Combine(PathHelper.GetParent(AppContext.BaseDirectory, 7), "vue-vben-admin", "apps", "web-naive", "src", "api");
        TypeScriptGenerator generator = new TypeScriptGenerator(PathHelper.Combine(PathHelper.GetParent(AppContext.BaseDirectory, 7),
            "vue-vben-admin", "apps", "web-naive", "src", "api"));
        generator.GenerateTypeScript();

        // 生成function文件
        var functions = MetadataHelper.GetTenantAppRights();
        var rightJSON = JsonConvert.SerializeObject(functions);
        var rightsPath = PathHelper.Combine(apiPath, "app");
        Directory.CreateDirectory(rightsPath);
        var rightsFile = PathHelper.Combine(rightsPath, "app.json");
        File.WriteAllText(rightsFile, rightJSON);
    }
}