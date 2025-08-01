using Simple.Interfaces.Dtos;

namespace Simple.Interfaces;

public interface IRoleService
{
    Task<List<RoleRightDto>> GetUserMenuRights(Guid userId, string application, string menuCode);


    Task<List<RoleRightDto>> GetUserMenuRights(List<Guid> userIds, string application, string menuCode);
}