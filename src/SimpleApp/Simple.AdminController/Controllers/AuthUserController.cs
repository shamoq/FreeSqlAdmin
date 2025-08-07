using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Simple.AdminApplication.Application.UserMng;
using Simple.AdminApplication.DbContexts;
using Simple.AdminApplication.Dtos;
using Simple.AspNetCore.Controllers;
using Simple.AspNetCore.Services;
using Simple.Utils.Extensions;
using Simple.Utils.Models.Tenants;

namespace Simple.AdminController.Controllers
{
    /// <summary>
    /// 初始化
    /// </summary>
    public class AuthUserController : AppAuthController
    {
        private RoleService _roleService;
        private UserContextService _userContextService;
        private IHostEnvironment _hostEnvironment;

        public AuthUserController(RoleService roleService, UserContextService userContextService,
            IHostEnvironment hostEnvironment)
        {
            _roleService = roleService;
            this._userContextService = userContextService;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>获取授权码</summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult> Codes()
        {
            var application = "appContract"; // 暂时默认写死

            // 获取用户权限
            var user = _userContextService.GetUser();
            var rights = await _roleService.GetUserRoles(user.Id, application);
            var codes = rights.Select(t => t.NavCode)
                .Union(rights.Select(t => t.MenuCode))
                .Union(rights.Select(t => t.ActionCode))
                .Distinct().ToList();

            // 获取权限信息
            string rightJson = null;

            if (string.IsNullOrEmpty(rightJson))
            {
                // 读取嵌入的资源文件
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"Simple.AdminController.Resources.contractrights.json";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    rightJson = await reader.ReadToEndAsync();
                }
            }

            var rightList = JsonConvert.DeserializeObject<List<RoleRightJsonItem>>(rightJson);

            if (TenantContext.CurrentTenant.ExpirationTime < DateTime.UtcNow)
            {
                // 移除所有非query的动作点
                rightList.RemoveAll(t => t.Type == "action" && t.Code != "query");
            }

            // 生成授权码
            var list = new List<int>();
            foreach (var right in rightList)
            {
                if (user.IsAdmin == 1)
                {
                    list.Add(1);
                }
                else
                {
                    list.Add(codes.Contains(right.Code) ? 1 : 0);
                }
            }

            return ApiResult.Success(string.Join("", list));
        }

        /// <summary>登录效期心跳</summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult Health()
        {
            var user = _userContextService.GetUser();
            return ApiResult.Success(user != null);
        }

        /// <summary>用户信息</summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult UserInfo()
        {
            var user = _userContextService.GetUser();
            if (ValueHelper.IsNullOrEmpty(user))
                return ApiResult.Fail("登录信息不存在");

            TenantContext.AllTenants.TryGetValue(user.TenantId.AsGuid(), out var tenantInfo);

            var day = Math.Ceiling((tenantInfo.ExpirationTime - DateTime.Now).TotalDays);
            if (day < 0)
            {
                user.SetAttributeValue("tenantDay", -1);
            }
            else if (day <= 15)
            {
                user.SetAttributeValue("tenantDay", day);
            }

            user.SetAttributeValue("tenantType", tenantInfo?.TenantType);
            user.SetAttributeValue("tenantName", tenantInfo?.TenantFullName);

            user.SetAttributeValue("tenantDay", day);
            return ApiResult.Success(user);
        }
    }
}