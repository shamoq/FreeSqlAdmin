using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Simple.AdminApplication.SysMng.Dto;
using Simple.AdminApplication.SysMng.Entities;
using Simple.AdminApplication.SysMng.Enums;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminApplication.SysMng.Controllers
{
    [Permission("ParamList", "系统管理")]
    public class ParamController : AppAuthController
    {
        private ParamValueService _paramValueService;

        public ParamController(ParamValueService paramValueService)
        {
            _paramValueService = paramValueService;
        }

        private List<ParamGroupDto> GetParamConfig(Guid id)
        {
            var file = PathHelper.Combine(AppContext.BaseDirectory, "metadata", "ParamConfig",
                $"{id}.metadata.json");
            if (!System.IO.File.Exists(file))
            {
                throw new Exception("参数配置文件不存在");
            }

            var json = System.IO.File.ReadAllText(file);
            var list = JsonConvert.DeserializeObject<List<ParamGroupDto>>(json);
            return list;
            // var item = new ParamConfigDto()
            // {
            //     Name = "Wps配置信息",
            //     Description = "配置信息联系管理员获取",
            //     ParamInfos = new List<ParamInfoDto>(){
            //                 new ParamInfoDto()
            //                 {
            //                     ParamCode="wps_enable", ParamName="是否启用", ParamType = ParamTypeEnum.Option,
            //                     Options = new List<OptionObjectDto>(){
            //                         new OptionObjectDto(){ Id = 1, Name = "是" },
            //                         new OptionObjectDto(){ Id = 0, Name = "否" },
            //                     }
            //                 },
            //                 new ParamInfoDto(){ ParamCode="wps_user", ParamName="Wps用户标识", ParamType = ParamTypeEnum.String },
            //                 new ParamInfoDto(){ ParamCode="wps_appId", ParamName="AppId", ParamType = ParamTypeEnum.String },
            //                 new ParamInfoDto(){ ParamCode="wps_appKey", ParamName="AppKey", ParamType = ParamTypeEnum.Password },
            //                 new ParamInfoDto(){ ParamCode="wps_callbackUrl", ParamName="回调地址", ParamType = ParamTypeEnum.String },
            //                }
            // };
            // return new List<ParamConfigDto> { item };
        }

        [HttpPost]
        [Permission("query", "获取参数")]
        public async Task<ApiResult> GetParams(ParameterModel input)
        {
            // 要加载的参数分组
            var wpsConfigId = new Guid("f90552cf-f5cc-45be-6868-08dd9cd81c24");
            var navs = new List<ParamNavDto>()
            {
                new ParamNavDto()
                {
                    Id = wpsConfigId,
                    Name = "配置类参数",
                    Description = "配置类参数",
                },
            };

            var paramList = GetParamConfig(wpsConfigId);

            var paramValues = await _paramValueService.GetList(t => t.ScopeId == null);

            GetParamConfigValue(paramList, paramValues);

            return ApiResult.Success(new
            {
                navs,
                paramConfigId = wpsConfigId,
                paramConfig = paramList,
            });
        }

        private static void GetParamConfigValue(List<ParamGroupDto> paramList, List<SysParamValue> paramValues)
        {
            var paramConfigs = paramList.Where(t => t.Items != null)
                .SelectMany(t => t.Items)
                .ToList();
            foreach (var item in paramConfigs)
            {
                item.ParamTypeText = StaticClassEnumHelper.GetDescription(item.ParamType, typeof(ParamTypeEnum));
                var paramValue = paramValues.FirstOrDefault(t => t.ParamCode == item.ParamCode)?.Value;
                if (paramValue != null)
                {
                    // 如果是选项类型，需要将value转换成选项的值类型
                    if (item.ParamType == ParamTypeEnum.Option)
                    {
                        var option =
                            item.Options.FirstOrDefault(t => t.Id.ToString() == paramValue);
                        if (option != null)
                        {
                            item.Value = TypeConvertHelper.ConvertType(paramValue, option.Id.GetType());
                        }
                    }
                    else
                    {
                        item.Value = paramValue;
                    }
                }
            }
        }

        /// <summary>
        /// 保存参数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Permission("save", "保存参数")]
        public async Task<ApiResult> SaveParamConfig(ParameterModel input)
        {
            if (input.Id == null || input.Id == Guid.Empty)
            {
                throw new Exception("参数错误");
            }

            var paramGroupList = GetParamConfig(input.Id.Value);
            var paramList = paramGroupList.SelectMany(t => t.Items).ToList();
            await _paramValueService.SaveParam(paramList, input.AdditionalData);
            return ApiResult.Success();
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        [HttpPost]
        [Permission("query", "获取参数")]
        public async Task<ApiResult> GetParamValue(ParameterModel input)
        {
            var code = input.GetAttributeValue<string>("code");
            if (string.IsNullOrEmpty(code))
            {
                return ApiResult.Fail("参数错误");
            }

            var paramValue = await _paramValueService.GetList(t => t.ParamCode == code);
            return ApiResult.Success(paramValue.FirstOrDefault()?.Value);
        }
    }
}