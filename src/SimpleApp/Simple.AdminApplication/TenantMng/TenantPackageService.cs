using Simple.AdminApplication.Common;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AdminApplication.UserMng.Entities;
using Simple.AdminApplication.UserMng;
using Simple.Utils.Exceptions;
using Mapster;
using Simple.AdminApplication.TenantMng.Interfaces;

namespace Simple.AdminApplication.TenantMng
{
    [Scoped]
    public class TenantPackageService : BaseCurdService<TenantPackage>
    {
        private TenantPackageRightService _tenantPackageRightService;
        private ITenantService _tenantService;

        public TenantPackageService(TenantPackageRightService tenantRightPackageService, ITenantService tenantService)
        {
            _tenantPackageRightService = tenantRightPackageService;
            _tenantService = tenantService;
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            // 判断是否有租户
            var tenants = await _tenantService.GetList(t => t.TenantPackageId == id);
            if (tenants.Any())
            {
                throw new CustomException("套餐包下有租户，不允许删除");
            }

            await base.DeleteAsync(id);
            await _tenantPackageRightService.DeleteAsync(t => t.TenantPackageId == id);
            return true;
        }

        public async Task Grant(TenantGrantInput input)
        {
            var tenantPackage = await FindAsync(input.TenantPackageId);
            if (tenantPackage is null) throw new CustomException("没有找到租户套餐信息");

            var tenantRightArray = await _tenantService.GetTenantGrantArray();
            var result = tenantRightArray.Where(t => input.ActionCodes.Contains(t.ActionCode))
                .ToList();

            var newRights = result.Adapt<List<TenantPackageRight>>();
            foreach (var item in newRights)
            {
                item.TenantPackageId = input.TenantPackageId;
            }

            await _tenantPackageRightService.DeleteAsync(t => t.TenantPackageId == input.TenantPackageId);
            await _tenantPackageRightService.AddAsync(newRights);
        }

        /// <summary>根据套餐包获取授权菜单</summary>
        public async Task<List<TenantGrantRightDto>> GetGrants(Guid tenantPackageId)
        {
            var rights = await _tenantPackageRightService.GetList(t => t.TenantPackageId == tenantPackageId);
            var dtos = rights.Adapt<List<TenantGrantRightDto>>();
            return dtos;
        }
    }
}