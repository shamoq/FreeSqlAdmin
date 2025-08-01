using Simple.Interfaces.Dtos;

namespace Simple.Interfaces;

public interface IParamValueService
{
    Task<WpsSettingDto> GetWpsSetting();
}