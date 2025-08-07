using Simple.Interfaces.Dtos;

namespace Simple.Interfaces;

public interface IOrgService
{
    Task<OrgEntityDto> GetById(Guid id);
}