using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AdminApplication.Enums;
using Simple.AdminController.Models;
using Simple.Interfaces;
using Simple.Interfaces.Dtos;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Simple.AdminApplication.Application.Workflows;
using Simple.AdminApplication.Application.Wps.Entities;
using Simple.AdminApplication.Dtos;
using Simple.Utils.Extensions;
using static Simple.AdminController.Controllers.TemplateController;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Models.Dto;
using Simple.Utils.Exceptions;

namespace Simple.AdminController.Controllers
{
    [ApiController]
    public class TemplateController : AppCurdController<Template, TemplateDto>
    {
        private readonly TemplateService _templateService;
        private readonly CustomFieldService customFieldService;
        private readonly ContractTypeService contractTypeService;
        private readonly BpmTemplateStepService _bpmTemplateStepService;

        public TemplateController(TemplateService templateService, CustomFieldService customFieldService,
            ContractTypeService contractTypeService, BpmTemplateStepService bpmTemplateStepService)
        {
            this._templateService = templateService;
            this.customFieldService = customFieldService;
            this.contractTypeService = contractTypeService;
            this._bpmTemplateStepService = bpmTemplateStepService;
        }

        [HttpPost]
        public override async Task<ApiResult> Page(QueryRequestInput input)
        {
            var search = input.GetAttributeValue<string>("search");

            var contractTypeId = input.GetAttributeValue<Guid?>("contractTypeId");
            var contractTypes = await contractTypeService.GetList(t => true);

            // 查找选中分类的所有子级
            var searchContractTypeIds = new List<Guid>();
            if (contractTypeId != null)
            {
                var treeBuilder = new TreeBuilder<ContractType>(contractTypes);
                var children = treeBuilder.GetChildren(contractTypeId.Value);
                searchContractTypeIds = children.Select(t => t.Id).ToList();
            }

            input.AdditionalExpression = new ExpressionHolder<Template>()
                .AddIf(searchContractTypeIds.Count > 0, t => searchContractTypeIds.Contains(t.ContractTypeId))
                .AddIf(!string.IsNullOrEmpty(search), t => t.Name.Contains(search) || t.Remark.Contains(search));

            var (total, data) = await _templateService.Page(input);

            foreach (var dto in data)
            {
                var item = contractTypes.FirstOrDefault(t => t.Id == dto.ContractTypeId);
                dto.SetAttributeValue("ContractTypeName", item?.Name);
                dto.SetAttributeValue("ContractTypeFullName", item?.FullName);
                dto.SetAttributeValue("templateTypeText", StaticClassEnumHelper.GetDescription(dto.TemplateType, typeof(TemplateTypeEnum)));
            }

            return ApiResult.Success(new { data, total });
        }

        [HttpPost]
        public async Task<ApiResult> GetReleaseTemplates(QueryRequestInput input)
        {
            var search = input.GetAttributeValue<string>("search");

            var contractTypeId = input.GetAttributeValue<Guid?>("contractTypeId");
            var contractTypes = await contractTypeService.GetList(t => true);

            // 查找选中分类的所有子级
            var searchContractTypeIds = new List<Guid>();
            if (contractTypeId != null)
            {
                var treeBuilder = new TreeBuilder<ContractType>(contractTypes);
                var children = treeBuilder.GetChildren(contractTypeId.Value);
                searchContractTypeIds = children.Select(t => t.Id).ToList();
            }

            input.AdditionalExpression = new ExpressionHolder<Template>()
                .AddIf(true, t => t.ReleaseStatus == (int)ReleaseStatusEnum.Yes)
                .AddIf(searchContractTypeIds.Count > 0, t => searchContractTypeIds.Contains(t.ContractTypeId))
                .AddIf(!string.IsNullOrEmpty(search), t => t.Name.Contains(search) || t.Remark.Contains(search));

            var (total, data) = await _templateService.Page(input);

            var dtos = data.Adapt<List<TemplateDto>>();
            foreach (var dto in dtos)
            {
                var item = contractTypes.FirstOrDefault(t => t.Id == dto.ContractTypeId);
                dto.ContractTypeName = item?.Name;
                dto.ContractTypeFullName = item?.FullName;
                dto.SetAttributeValue("templateTypeText", StaticClassEnumHelper.GetDescription(dto.TemplateType, typeof(TemplateTypeEnum)));
            }

            return ApiResult.Success(new { data = dtos, total });
        }

        /// <summary>
        /// 保存详情
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> SaveTemplate(TemplateSaveInput input)
        {
            if (input.Template?.Id == null)
            {
                throw new CustomException("模板参数不存在");
            }

            var result = await _templateService.SaveTemplate(input);
            return ApiResult.Success(result);
        }

        /// <summary>
        /// 查询详情
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> GetDetail(ParameterModel input)
        {
            if (input.Id == null)
            {
                return ApiResult.Fail("Id为空");
            }

            var template = await _templateService.FindById(input.Id.Value);
            var fields = await customFieldService.GetContractFields(template.Id);
            var contractType = await contractTypeService.FindBy(t => t.Id == template.ContractTypeId);

            var detailDto = new TemplateDetailDto();
            detailDto.Template = template.Adapt<TemplateDto>();
            detailDto.Template.ContractTypeName = contractType?.Name;

            detailDto.Fields = fields;
            detailDto.Steps = await _bpmTemplateStepService.GetStepsByBusinessId(template.Id);

            return ApiResult.Success(detailDto);
        }
    }
}