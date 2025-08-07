using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Enums;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Extensions;

namespace Simple.AdminController.Controllers
{
    public class ParamController : AppAuthController
    {
        private ParamValueService _paramValueService;

        public ParamController(ParamValueService paramValueService)
        {
            _paramValueService = paramValueService;
        }

        private List<ParamConfigDto> GetParamConfigs()
        {
            var item = new ParamConfigDto()
            {
                Name = "Wps配置信息",
                Description = "配置信息联系管理员获取",
                ParamInfos = new List<ParamInfoDto>(){
                            new ParamInfoDto(){ ParamCode="wps_user", ParamName="Wps用户标识", ParamType = ParamTypeEnum.String },
                            new ParamInfoDto(){ ParamCode="wps_appId", ParamName="AppId", ParamType = ParamTypeEnum.String },
                            new ParamInfoDto(){ ParamCode="wps_appKey", ParamName="AppKey", ParamType = ParamTypeEnum.Password },
                            new ParamInfoDto(){ ParamCode="wps_callbackUrl", ParamName="回调地址", ParamType = ParamTypeEnum.String },
                           }
            };
            return new List<ParamConfigDto> { item };
        }

        [HttpPost]
        public async Task<ApiResult> GetParams()
        {
            var list = new List<ParamGroupDto>(){
                 new ParamGroupDto(){
                      Id="01775d9c-03e4-11f0-9aa0-00155db7024f".AsGuid(),
                      Name = "配置类参数",
                 },
            };
            var paramValues = await _paramValueService.GetList(t => t.ScopeId == null);

            var paramConfigs = GetParamConfigs();

            foreach (var paramGroup in paramConfigs)
            {
                foreach (var item in paramGroup.ParamInfos)
                {
                    item.ParamTypeText = StaticClassEnumHelper.GetDescription(item.ParamType, typeof(ParamTypeEnum));
                    item.Value = paramValues.FirstOrDefault(t => t.ParamCode == item.ParamCode)?.Value;
                }
            }

            return ApiResult.Success(new
            {
                groups = list,
                paramConfig = paramConfigs,
            });
        }

        /// <summary>
        /// 保存参数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> SaveParamConfig(ParameterModel input)
        {
            var paramGroupList = GetParamConfigs();
            var paramList = paramGroupList.SelectMany(t => t.ParamInfos).ToList();
            await _paramValueService.SaveParam(paramList, input.AdditionalData);
            return ApiResult.Success();
        }
    }
}