using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.Workflows;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AdminApplication.Enums;
using Simple.AdminController.Models;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Models.Dto;
using System.IO;
using Simple.AdminApplication.Application.OssService;
using Simple.AspNetCore.Services;
using Simple.Utils.Consts;

namespace Simple.AdminController.Controllers
{
    public class ContractController : AppCurdController<Contract, ContractDto>
    {
        private TemplateService _templateService;
        private ContractTypeService _contractTypeService;
        private ProviderService _providerService;
        private SignPartyService _signPartyService;
        private ContractService _contractService;
        private UserContextService _userContextService;

        public ContractController(TemplateService templateService, ContractTypeService contractTypeService,
            ProviderService providerService, SignPartyService signPartyService, ContractService contractService)
        {
            _templateService = templateService;
            _contractTypeService = contractTypeService;
            _providerService = providerService;
            _signPartyService = signPartyService;
            _contractService = contractService;
        }

        [HttpPost]
        public override async Task<ApiResult> Page(QueryRequestInput pageRequest)
        {
            var isMyCreate = pageRequest.GetAttributeValue<bool?>("isMyCreate");
            var isMyApprove = pageRequest.GetAttributeValue<bool?>("isMyApprove");
            var isContract = pageRequest.GetAttributeValue<bool?>("isContract");

            var sencen = "";
            if (isMyCreate == true)
            {
                sencen = "isMyCreate";
            }
            else if (isMyApprove == true)
            {
                sencen = "isMyApprove";
            }
            else if (isContract == true)
            {
                sencen = "allcontract";
            }

            var (total, list) = await _contractService.Page(pageRequest, sencen);
            var data = list.Adapt<List<ContractDto>>();

            // 加载其他字段
            var templateIds = data.Where(t => t.TemplateId != null).Select(t => t.TemplateId).ToList();
            if (templateIds.Count > 0)
            {
                var templates = await _templateService.GetList(t => templateIds.Contains(t.Id));
                var contractTypeIds = templates.Select(t => t.ContractTypeId).ToList();
                var contractTypes = await _contractTypeService.GetList(t => contractTypeIds.Contains(t.Id));
                foreach (var item in data)
                {
                    if (item.TemplateId != null)
                    {
                        var template = templates.FirstOrDefault(t => t.Id == item.TemplateId);
                        if (template != null)
                        {
                            item.TemplateName = template.Name;
                            item.ContractTypeId = template.ContractTypeId;
                            item.ContractTypeName =
                                contractTypes.FirstOrDefault(t => t.Id == template.ContractTypeId)?.Name;
                            item.ContractTypeFullName =
                                contractTypes.FirstOrDefault(t => t.Id == template.ContractTypeId)?.FullName;
                        }
                    }
                }
            }

            return ApiResult.Success(new { total, data });
        }

        /// <summary>
        /// 保存合同详情
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> SaveContract(ContractDetailDto input)
        {
            var contract = await _contractService.SaveContract(input);
            return ApiResult.Success(contract.Id);
        }

        /// <summary>
        /// 获取合同详情
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> GetDetail(ParameterModel input)
        {
            var templateId = input.GetAttributeValue<Guid>("templateId");
            if (input.Id == null)
            {
                return ApiResult.Fail("参数错误");
            }

            var detailDto = await _contractService.GetContractDetail(input.Id.Value, templateId);

            return ApiResult.Success(detailDto);
        }

        /// <summary>
        /// 线下签署合同
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> Sign(ContractSignInput input)
        {
            if (input.Id == Guid.Empty || input.SignName == null || input.SignTime == null)
            {
                return ApiResult.Fail("参数错误");
            }

            var contract = await _contractService.Sign(input);
            var dto = contract.Adapt<ContractDto>();
            return ApiResult.Success(dto);
        }

        /// <summary>
        /// 获取合同签署人
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> GetSigners(ParameterModel input)
        {
            var type = input.GetAttributeValue<int>("type");
            var search = input.GetAttributeValue<string>("search");
            List<OptionDto> options;
            if (type == 1) // 公司
            {
                options = await _providerService.GetOptions(search);
            }
            else // 个人
            {
                options = await _signPartyService.GetOptions(search);
            }

            if (!string.IsNullOrEmpty(search) && options.Count == 0)
            {
                options.Add(new OptionDto() { Id = Guid.Empty, Name = search });
            }

            return ApiResult.Success(options);
        }
    }
}