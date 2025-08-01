using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simple.AdminApplication;
using Simple.AspNetCore.Controllers;
using Simple.EntityFrameworkCore;
using Simple.Interfaces;
using Simple.Interfaces.Dtos;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Simple.AdminController.Models;

namespace Simple.AdminController.Controllers
{
    public class DyamicQueryController : AppAuthController
    {
        private IDyamicQueryService dyamicQuery;

        /// <summary>
        ///
        /// </summary>
        public DyamicQueryController(IDyamicQueryService dyamicQuery)
        {
            this.dyamicQuery = dyamicQuery;
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> List(DyamicSqlInput input)
        {
            var list = dyamicQuery.GetList<Dictionary<string, object>>(input);
            return await Task.FromResult(ApiResult.Success(list));
        }

        /// <summary>
        /// 查询分页结果
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Page(DyamicSqlInput input)
        {
            var (total, data) = dyamicQuery.GetPageList<Dictionary<string, object>>(input);

            return await Task.FromResult(ApiResult.Success(new { data, total }));
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Single(DyamicSqlInput input)
        {
            var value = dyamicQuery.GetSingle<string>(input);

            return await Task.FromResult(ApiResult.Success(value));
        }

        /// <summary>
        /// 组合查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Combine(DyamicQueryCombineInput input)
        {
            var sqlHelper = new DyamicSqlQueryHelper();
            var dic = new Dictionary<string, object>();
            foreach (var item in input.Items)
            {
                if (item.QueryType == 1)
                {
                    var list = dyamicQuery.GetList<string>(item);
                    dic[item.Name] = list;
                }
                else
                {
                    var value = dyamicQuery.GetSingle<string>(item);
                    dic[item.Name] = value;
                }
            }
            return await Task.FromResult(ApiResult.Success(dic));
        }
    }
}