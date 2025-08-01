using Simple.Interfaces.Dtos;

namespace Simple.Interfaces;

public interface IUserService
{
    Task<List<UserEntityDto>> GetUserByIds(List<Guid> userIds);
}