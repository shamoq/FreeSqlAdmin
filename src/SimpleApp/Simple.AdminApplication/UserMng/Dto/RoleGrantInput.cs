namespace Simple.AdminApplication.UserMng.Dto;

public class RoleGrantInput
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public Guid RoleId { get; set; }

    public List<string> ActionCodes { get; set; }
}