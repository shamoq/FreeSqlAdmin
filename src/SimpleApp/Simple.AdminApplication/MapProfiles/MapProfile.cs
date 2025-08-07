using Mapster;
using Simple.AdminApplication.SysMng.Entities;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AdminApplication.UserMng.Entities;
using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.MapProfiles;

public class MapProfile
{
    public static void Configure(TypeAdapterConfig config)
    {
        config.ForType<BaseDto, DefaultEntity>()
            .Ignore(dest => dest.Creator)
            .Ignore(dest => dest.CreatedTime)
            .Ignore(dest => dest.CreatedId)
            .Ignore(dest => dest.UpdatedTime)
            .Ignore(dest => dest.Updator)
            .Ignore(dest => dest.UpdatedId);

        config.NewConfig<SysFileDocument, FileDocumentDto>()
            .Map(dest => dest.DocumentId, src => src.Id);

        config.NewConfig<SysFileDocument, FileDocumentDto>()
         .Map(dest => dest.DocumentId, src => src.Id);

        config.NewConfig<FileDocumentDto, SysFileDocument>()
           .Map(dest => dest.Id, src => src.DocumentId);

        config.NewConfig<SysUserDto, SysUser>().AfterMapping((dest, src) =>
        {
            var newPassword = dest.GetAttributeValue<string>("newpassword");
            if (!string.IsNullOrEmpty(newPassword))
            {
                src.Password = newPassword;
            }
        });
        
    }
}