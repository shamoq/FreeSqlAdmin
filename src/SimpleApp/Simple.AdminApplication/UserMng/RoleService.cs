using Mapster;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.TenantMng.Interfaces;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AdminApplication.UserMng.Entities;
using Simple.FreeSql.Helpers;
using Simple.Interfaces.Dtos;
using Simple.Utils.Exceptions;

namespace Simple.AdminApplication.UserMng
{
    /// <summary>
    /// 角色服务
    /// </summary>
    [Scoped]
    public class RoleService : BaseCurdService<SysRole>
    {
        private RoleRightService roleRightService;
        private User2RoleService user2RoleService;
        private ITenantService _tenantService;

        public RoleService(User2RoleService user2RoleService, RoleRightService roleRightService, ITenantService tenantService)
        {
            this.roleRightService = roleRightService;
            _tenantService = tenantService;
            this.user2RoleService = user2RoleService;
        }

        /// <summary>角色授权菜单权限</summary>
        public async Task Grant(RoleGrantInput input)
        {
            var role = await FindAsync(input.RoleId);
            if (role is null) throw new CustomException("没有找到角色信息");

            var tenantRightArray = await _tenantService.GetTenantGrantArray();
            var result = tenantRightArray.Where(t => input.ActionCodes.Contains(t.ActionCode))
                .ToList();
            var newRights = result.Adapt<List<SysRoleRight>>();
            foreach (var item in newRights)
            {
                item.RoleId = input.RoleId;
            }

            await roleRightService.DeleteAsync(t => t.RoleId == input.RoleId);
            await roleRightService.AddAsync(newRights);
        }

        /// <summary>获取角色授权菜单权限</summary>
        public async Task<List<RoleGrantRightDto>> GetRoleGrants(Guid roleId)
        {
            var rights = await roleRightService.GetList(t => t.RoleId == roleId);
            var dtos = rights.Adapt<List<RoleGrantRightDto>>();
            return dtos;
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await base.FindAsync(id);
            // 校验是否有子级角色
            var hasRoles = await table.Select.AnyAsync(t => t.ParentId == id);
            if (hasRoles)
                throw new CustomException("请先删除子级角色");
            await table.DeleteAsync(entity);

            // 删除授权数据
            await roleRightService.DeleteAsync(t => t.RoleId == id);

            return true;
        }

        public override async Task<SysRole> Save(SysRole entity, bool isForceAdd = false)
        {
            // 校验角色名称不能重复
            var hasRole = await table.Select.AnyAsync(t =>
                t.Name == entity.Name && t.Id != entity.Id && t.ParentId == entity.ParentId);
            if (hasRole)
                throw new CustomException("角色名称已存在");

            await new TreeEntityHelper<SysRole>(fsql).Save(entity);

            return entity;
        }

        public async Task SetUserRoles(Guid userId, List<Guid> roleIds)
        {
            await user2RoleService.DeleteAsync(t => t.UserId == userId);
            if (roleIds.Count > 0)
            {
                var user2Roles = roleIds.Select(t => new SysUser2Role() { UserId = userId, RoleId = t }).ToList();
                await user2RoleService.AddAsync(user2Roles);
            }
        }

        /// <summary>
        /// 获取有权限的角色名称
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserRoleDto>> GetUserRoles(List<Guid> userIds)
        {
            var userRoles = await user2RoleService.GetList(t => userIds.Contains(t.UserId));
            var roleIds = userRoles.Select(t => t.RoleId).ToList();
            var roles = await table.Where(t => roleIds.Contains(t.Id)).ToListAsync();
            var roleDic = roles.ToDictionary(t => t.Id, t => t.Name);
            var list = userRoles.Select(t =>
            {
                var dto = t.Adapt<UserRoleDto>();
                dto.RoleName = roleDic.TryGetValue(t.RoleId, out var roleName) ? roleName : string.Empty;
                return dto;
            }).ToList();
            return list;
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleRightDto>> GetUserRoles(Guid userId)
        {
            var userRoles = await user2RoleService.GetList(t => userId == t.UserId);
            var roleIds = userRoles.Select(t => t.RoleId).ToList();

            var rightLists =
                await roleRightService.GetList(t => roleIds.Contains(t.RoleId));

            var rights = rightLists
                .Select(t => new RoleRightDto()
                {
                    NavCode = t.NavCode,
                    ActionCode = t.ActionCode,
                    Application = t.Application,
                    MenuCode = t.MenuCode
                })
                .Distinct()
                .ToList();
            return rights;
        }

        /// <summary>
        /// 获取用户菜单权限
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleRightDto>> GetUserMenuRights(Guid userId, string application, string menuCode)
        {
            var userRoles = await user2RoleService.GetList(t => userId == t.UserId);
            var roleIds = userRoles.Select(t => t.RoleId).ToList();

            var rightsList = await roleRightService.GetList(t => roleIds.Contains(t.RoleId) &&
                                                                 t.Application == application
                                                                 && t.MenuCode == menuCode);
            var rights = rightsList.Select(t => new RoleRightDto()
            {
                NavCode = t.NavCode,
                ActionCode = t.ActionCode,
                Application = t.Application,
                MenuCode = t.MenuCode
            })
                .Distinct()
                .ToList();
            foreach (var item in rights)
            {
                item.UserId = userId;
            }

            return rights;
        }

        /// <summary>
        /// 获取用户菜单权限
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleRightDto>> GetUserMenuRights(List<Guid> userIds, string application, string menuCode)
        {
            var list = new List<RoleRightDto>();

            var userRoles = await user2RoleService.GetList(t => userIds.Contains(t.UserId));
            var roleIds = userRoles.Select(t => t.RoleId).ToList();

            var rights = await roleRightService.GetList(t =>
                roleIds.Contains(t.RoleId) && t.Application == application && t.MenuCode == menuCode);

            foreach (var userId in userIds)
            {
                var roleGuids = userRoles.Where(t => t.UserId == userId)
                    .Select(t => t.RoleId).ToList();
                var roleRights = rights.Where(t => roleGuids.Contains(t.RoleId)).ToList();
                list.AddRange(roleRights.Select(t => new RoleRightDto()
                {
                    NavCode = t.NavCode,
                    ActionCode = t.ActionCode,
                    Application = t.Application,
                    MenuCode = t.MenuCode,
                    UserId = userId
                }).Distinct().ToList());
            }

            return list;
        }
    }
}