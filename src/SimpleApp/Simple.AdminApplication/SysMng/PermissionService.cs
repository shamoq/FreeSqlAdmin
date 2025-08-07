using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Interfaces;
using Simple.AdminApplication.UserMng;
using Simple.Interfaces.Dtos;

namespace Simple.AdminApplication.SysMng;

[Scoped]
public class PermissionService
{
    private RoleService _roleService;
    private ITenantService _tenantService;
    private AppProfileDataTable _appProfileDataTable;

    public PermissionService(IDistributedCache cache, ITenantService tenantService, RoleService roleService)
    {
        _appProfileDataTable = HostServiceExtension.AppProfileData["permissions"];
        _tenantService = tenantService;
        _roleService = roleService;
    }

    /**
     * 初始化菜单权限
     */
    public async Task<List<RoleRightDto>> InitializePermissionsAsync(Guid userId)
    {
        var tenantPackageRights = await _tenantService.GetTenantGrantArray();

        var allAppRightsCode = tenantPackageRights.Select(t => t.ActionCode).ToList();
        
        List<RoleRightDto> rights ;

        // 获取租户授权菜单
        if (TenantContext.CurrentUser.IsAdmin != 1)
        {
            // 获取用户权限
            rights = await _roleService.GetUserRoles(userId);

            // 过滤出当前应用的权限
            var rightList = allAppRightsCode.Count == 0
                ? rights
                : rights.Where(t => allAppRightsCode.Contains(t.ActionCode)).ToList();
        }
        else
        {
            rights = tenantPackageRights.Adapt<List<RoleRightDto>>();
        }
        
        var key = $"PermissionCache_{TenantContext.CurrentUser.Id}";
        _appProfileDataTable.Set(key, rights);
        return rights;
    }

    /// <summary>
    /// 获取菜单权限
    /// </summary>
    /// <returns></returns>
    public async Task<List<RoleRightDto>> GetUserPermissionsAsync(Guid userId)
    {
        var key = $"PermissionCache_{TenantContext.CurrentUser.Id}";
        var permissionData = _appProfileDataTable.Get<List<RoleRightDto>>(key);
        if (permissionData != null)
        {
            return permissionData;
        }

        permissionData = await InitializePermissionsAsync(userId);
        return permissionData;
    }

    /// <summary>
    /// 获取菜单权限代码
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<string>> GetUserPermissionCodeAsync(Guid userId)
    {
        var rights = await GetUserPermissionsAsync(userId);
        var codes = rights.Select(t => t.NavCode)
            .Union(rights.Select(t => t.MenuCode))
            .Union(rights.Select(t => t.ActionCode))
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct().ToList();
        return codes;
    }

    /// <summary>
    /// 清空指定用户的权限缓存
    /// </summary>
    /// <param name="userId"></param>
    public async Task ClearPermissionCacheAsync(Guid userId)
    {
        var key = $"PermissionCache_{TenantContext.CurrentUser.Id}";
        _appProfileDataTable.Remove(key);
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 清空所有权限缓存
    /// </summary>
    public async Task ClearPermissionCacheAsync()
    {
        _appProfileDataTable.RemoveAll();
        await Task.CompletedTask;
    }
}