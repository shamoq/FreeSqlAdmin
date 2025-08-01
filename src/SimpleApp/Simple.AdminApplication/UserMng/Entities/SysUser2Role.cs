namespace Simple.AdminApplication.UserMng.Entities
{
    public class SysUser2Role : DefaultTenantEntity
    {
        /// <summary>
        /// 用户GUID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid RoleId { get; set; }
    }
}