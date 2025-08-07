using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.UserMng.Dto
{
    public class UserRoleDto : BaseDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
    }
}