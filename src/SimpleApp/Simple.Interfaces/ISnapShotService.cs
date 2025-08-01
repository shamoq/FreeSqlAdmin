using Simple.Interfaces.Dtos;

namespace Simple.Interfaces;

public interface ISnapShotService
{
    Task<Guid> SaveSnapByBusinessId(Guid? businessId, string type, object value);

    Task DeleteSnapByBusinessId(Guid businessId, string type);


    Task<T> GetSnapByBusinessId<T>(Guid businessId, string type);


    Task<SnapShotEntityDto> AppendSnapByBusinessId(Guid businessId, string type, object value);
}