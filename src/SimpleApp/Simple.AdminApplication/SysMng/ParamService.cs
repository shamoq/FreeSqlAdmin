using FreeSql;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.SysMng.Dto;
using Simple.AdminApplication.SysMng.Entities;
using Simple.Interfaces;
using Simple.Interfaces.Dtos;

namespace Simple.AdminApplication.SysMng
{
    [Scoped]
    public class ParamValueService : BaseCurdService<SysParamValue>, IParamValueService
    {
        public async Task SaveParam(List<ParamItemDto> paramList, Dictionary<string, object> paramValue)
        {
            var existingParams = await table.Where(x => x.ScopeId == null).ToListAsync();

            foreach (var param in paramList)
            {
                var existingParam = existingParams.FirstOrDefault(x => x.ParamCode == param.ParamCode);
                if (existingParam != null)
                {
                    // 更新参数值
                    if (paramValue.TryGetValue(param.ParamCode, out var newValue))
                    {
                        existingParam.Value = newValue?.ToString()?.Trim();
                        existingParam.ParamType = param.ParamType;
                        await UpdateAsync(existingParam);
                    }
                }
                else
                {
                    // 插入新的参数值
                    if (paramValue.TryGetValue(param.ParamCode, out var newValue))
                    {
                        var newParam = new SysParamValue
                        {
                            ParamCode = param.ParamCode,
                            Value = newValue?.ToString()?.Trim(),
                            ParamType = param.ParamType,
                            ScopeId = null // 根据需要设置 ScopeId
                        };
                        await table.InsertAsync(newParam);
                    }
                }
            }
        }

        public async Task<WpsSettingDto> GetWpsSetting()
        {
            var existingParams = await table.Where(x => x.ScopeId == null).ToListAsync();
        
            return new WpsSettingDto()
            {
                AppId = existingParams.FirstOrDefault(x => x.ParamCode == "wps_appId")?.Value,
                AppKey = existingParams.FirstOrDefault(x => x.ParamCode == "wps_appKey")?.Value,
                FileDocKey = existingParams.FirstOrDefault(x => x.ParamCode == "wps_fileDocKey")?.Value,
                CallbackUrl = existingParams.FirstOrDefault(x => x.ParamCode == "wps_callbackUrl")?.Value
            };
        }
    }
}