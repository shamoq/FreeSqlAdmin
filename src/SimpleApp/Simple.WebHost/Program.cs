using Microsoft.OpenApi.Models;
using Simple.WebHost;
using Swashbuckle.AspNetCore.Filters;
using Simple.AdminApplication.Helpers;
using Simple.AdminApplication.TenantMng.Middleware;

ConsoleHelper.Debug($"开始构建主机");

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["ConfigEnv"].ToString().ToUpper() == "DEV")
{
    ConsoleHelper.Debug($"检测到当前是开发环境，使用配置文件：Config/app_dev.json");
    builder.Configuration.AddJsonFile("Config/app_dev.json", true, true);
}
else
{
    ConsoleHelper.Debug($"检测到当前为正式环境，使用配置文件：Config/app.json");
    builder.Configuration.AddJsonFile("Config/app.json", true, true);
}

#region 添加服务到容器

ConfigHelper.Init(builder.Configuration); // 配置初始化
builder.Services.AddControllerConfig();

//配置Swagger - 仅在开发环境启用
if (builder.Configuration["ConfigEnv"].ToString().ToUpper() == "DEV")
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo { Title = "API接口", Version = "v1" });
        foreach (var item in HostServiceExtension.XmlCommentsFilePath())
        {
            option.IncludeXmlComments(item, true);
        }

        option.OperationFilter<SecurityRequirementsOperationFilter>();
        option.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
            Name = ConfigHelper.GetValue("TokenHeadKey"),
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });
    });
}

builder.Services.AddRedisCacheAndSession();
builder.Services.AddJwtAuth();
builder.Services.AddCustomerCors();

HostServiceExtension.BuildHostService(builder.Services);
ConsoleHelper.Debug($"主机服务配置完成，开始配置管道");

#endregion 添加服务到容器

//配置启动的管道服务
var app = builder.Build();
app.UseSession();
HostServiceExtension.BuildHostApp(app);
app.AddJobScheduler();

//配置Swagger - 仅在开发环境显示
if (builder.Configuration["ConfigEnv"].ToString().ToUpper() == "DEV" && ConfigHelper.GetValue<bool>("ShowSwagger"))
{
    ConsoleHelper.Waring($"Waring：主机开启了Swagger，有暴露API风险");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantMiddleware();

app.MapControllers();

// 开发环境生成前端代码
if (builder.Configuration["ConfigEnv"].ToString().ToUpper() == "DEV")
{
    FrontendHelper.Create();
}

ConsoleHelper.Debug($"配置主机管道完成，准备启动");

app.Run();