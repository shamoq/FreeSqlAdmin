using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AspNetCore.Controllers;
using Simple.AspNetCore.Services;
using Simple.Utils.Extensions;
using System.Reflection;
using System.Security.Cryptography;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.TenantMng.Helper;
using Simple.AdminApplication.TenantMng.Interfaces;
using Simple.Utils.Consts;

namespace Simple.AdminApplication.UserMng.Controllers
{
    /// <summary>
    /// 初始化
    /// </summary>
    public class AuthUserController : AppAuthController
    {
        private readonly PermissionService _permissionService;

        public AuthUserController(PermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>获取授权码</summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult> Codes()
        {
            var codes = await _permissionService.GetUserPermissionCodeAsync(TenantContext.CurrentUser.Id);

            // 返回授权编码后的数据
            var json = JsonConvert.SerializeObject(codes);
            var compress = await StringCompressHelper.CompressData(json);
            // var result = EncryptHelper.AesDecrypt(compress, "NXhMJmoyRDlQMGtmUiNlRw==", "YVEzQE42bUZ6N0J2WSo0SA==",
            //     CipherMode.CBC);
            return ApiResult.Success(codes);
        }

        /// <summary>登录效期心跳</summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult Health()
        {
            var user = TenantContext.CurrentUser;
            return ApiResult.Success(user != null);
        }

        /// <summary>用户信息</summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult UserInfo()
        {
            var user = TenantContext.CurrentUser;
            if (ValueHelper.IsNullOrEmpty(user))
                return ApiResult.Fail("登录信息不存在");

            if (TenantContext.CurrentTenant == null)
            {
                return ApiResult.Fail("租户不存在");
            }

            var time = TenantContext.CurrentTenant.ExpirationTime;
            if (time != null)
            {
                var day = Math.Ceiling((time.Value - DateTime.Now).TotalDays);
                if (day < 0)
                {
                    user.SetAttributeValue("tenantDay", -1);
                }
                else if (day <= 15)
                {
                    user.SetAttributeValue("tenantDay", day);
                }
                user.SetAttributeValue("tenantDay", day);
            }

            user.SetAttributeValue("tenantType", TenantContext.CurrentTenant.Type);
            user.SetAttributeValue("tenantName", TenantContext.CurrentTenant.Name);

            return ApiResult.Success(user);
        }
    }
}