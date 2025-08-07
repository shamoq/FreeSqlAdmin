using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.UserMng.Entities;
using Simple.Utils.Consts;

namespace Simple.AdminApplication.UserMng.SeedData
{
    public class SysOrgSeedData
    {
        public void Init(IFreeSql db)
        {
            var table = db.Select<SysOrg>();
            if (table.Any())
            {
                return;
            }

            var list = new List<SysOrg>();

            list.Add(new SysOrg()
            {
                Code = "jt",
                Name = "集团",
                FullName = "集团",
                Id = TenantContext.CurrentTenant.Id,
                OrgType = 1,
                ParentId = null,
                OrderCode = "0001",
                OrderFullCode = "0001",
            }
            );

            db.Insert<SysOrg>(list).ExecuteAffrows();
        }
    }
}