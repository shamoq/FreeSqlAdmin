using Simple.AdminApplication.Common;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.UserMng.Entities;
using Simple.Utils.Consts;

namespace Simple.AdminApplication.UserMng.SeedData
{
    public class SysUserSeedData
    {
        public void Init(IFreeSql fsql)
        {
            if (fsql.Select<SysUser>().Any())
            {
                return;
            }

            // fsql.Delete<SysUser>().Where(t => t.IsAdmin == 1).ExecuteAffrows();

            var list = new List<SysUser>();

            // 模拟前端传值，密码也是md5加密后的密码
            var password = EncryptHelper.MD5Encrypt("123456");

            var user = new SysUser()
            {
                Avatar = "",
                IsAdmin = 1,
                IsEnable = 1,
                Phone = "",
                UserName = "系统管理员",
                Email = "",
                Id = Guid.NewGuid(),
                Salt = "",
                UserCode = "admin",
                LastIpAdress = "",
                LastLoginTime = null,
                OrgId = TenantContext.CurrentTenant.Id,
            };
            user.Password = UserService.GetMd5Password(user, password);
            list.Add(user);

            fsql.Insert(list).ExecuteAffrows();
        }
    }
}