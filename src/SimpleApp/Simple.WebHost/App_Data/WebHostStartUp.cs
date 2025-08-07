using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Simple.AspNetCore.Helper;
using Simple.AspNetCore.Models;
using Simple.Job;
using System.Text;
using Simple.Utils.Extensions;
using Simple.Utils.Exceptions;

namespace Simple.WebHost
{
    /// <summary>主机通用配置</summary>
    public static class WebHostStartUp
    {
        private static async void Jobmanager_JobEnd(FluentScheduler.JobEndInfo obj)
        {
            if (obj.Name.IsNullOrEmpty()) return;
            // var jobInfo = new AdminApplication.Entitys.SysJobLog
            // {
            //     Name = obj.Name,
            //     StartTime = obj.StartTime,
            //     NextRun = obj.NextRun,
            //     Duration = $"{obj.Duration.Hours}时{obj.Duration.Minutes}分{obj.Duration.Seconds}秒"
            // };
            // await AppDomainEventDispatcher.PublishEvent(jobInfo);
            await Task.CompletedTask;
        }

        /// <summary>添加JWT格式token</summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddJwtAuth(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.RequireHttpsMetadata = false;
                    option.SaveToken = true;

                    var jwtSetting = ConfigHelper.GetValue<JwtSetting>("Jwt");
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true, // 验证签名密钥
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.ASCII.GetBytes(jwtSetting.SecurityKey)), // 使用配置中的安全密钥创建对称安全密钥
                        ValidIssuer = jwtSetting.Issuer, // 设置合法的签发者
                        ValidateIssuer = true, // 验证签发者
                        ValidateAudience = false, // 不验证接收者
                    };
                    option.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = e =>
                        {
                            var url = e.Request.Path;
                            // 从URL参数中获取token
                            var token = e.Request.Query["token"].ToString();
                            if (string.IsNullOrEmpty(token))
                            {
                                // 从Cookie中获取token
                                token = e.Request.Cookies["token"];
                            }
                            if (string.IsNullOrEmpty(token))
                            {
                                // 从Cookie中获取token
                                token = e.Request.Headers["X-Weboffice-Token"];
                            }

                            if (!string.IsNullOrEmpty(token))
                            {
                                e.Token = token.Replace("Bearer ", "");
                            }
                            return Task.CompletedTask;
                        },
                        // 验证通过事件
                        OnTokenValidated = context =>
                        {
                            // 登录请求不处理用户上下文
                            if (context.Request.Path.Value != null && context.Request.Path.Value.Contains("/api/auth/"))
                            {
                                return Task.CompletedTask;
                            }

                            // 从 JWT 中获取用户信息
                            if (context.SecurityToken is JsonWebToken jwtToken)
                            {
                                var loginUser = JWTHelper.GetPayload(jwtToken.Claims.ToArray());

                                // 将用户信息添加到 HttpContext.Items 中
                                context.HttpContext.Items["UserInfo"] = loginUser;

                                return Task.CompletedTask;
                            }
                            else
                            {
                                throw new CustomException("缺失认证信息");
                            }
                        }
                    };
                });
            return services;
        }

        /// <summary>添加到管道尾部，以便能注入到全部的服务</summary>
        /// <param name="app"></param>
        public static void AddJobScheduler(this IApplicationBuilder app)
        {
            var jobmanager = new JobSchedule();
            jobmanager.JobEnd += Jobmanager_JobEnd;
            jobmanager.Start();
        }
    }
}