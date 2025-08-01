using Simple.Interfaces.Dtos;

namespace Simple.Interfaces;

public interface ITemplateVersionService
{
    Task<TemplateVersionEntityDto> CreateWpsWordIfNull(Guid businessId);
    Task<TemplateVersionEntityDto> GetById(Guid id);
    
    Task<TemplateVersionEntityDto> GetByBusinessId(Guid id);
    
    Task<TemplateVersionEntityDto> SaveEntity(TemplateVersionEntityDto entity);

    Task<Stream> GetWordStream(Guid templateId, Dictionary<string, object> data,
        List<CustomFieldDto> fields);
    
    Task DeleteByBusinessId(Guid businessId);

    Task<TemplateVersionEntityDto> UploadOfficeTemplate(Guid businessId, Guid documentId);

    Task<TemplateVersionEntityDto> UploadOfficeTemplate(Guid businessId, string name, Stream fileStream);
}