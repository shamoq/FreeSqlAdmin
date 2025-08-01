using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Simple.Utils.Exceptions;
using Simple.Utils.Helper;
using Simple.Utils.Models;

namespace Simple.AspNetCore.Filters
{
    /// <summary>全局异常过滤器</summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment hostEnvironment;

        /// <summary>全局异常过滤器</summary>
        /// <param name="_hostEnvironment"></param>
        public GlobalExceptionFilter(IHostEnvironment _hostEnvironment)
        {
            this.hostEnvironment = _hostEnvironment;
        }

        /// <summary>全局异常触发</summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled) //如果异常没有处理
            {
                Console.WriteLine(context.Exception.ToString());

                var message = string.Empty;
                if (context.Exception is CustomException customException)
                {
                    message = customException.Message;
                }

                LogHelper.Error(context.Exception.Message, context.Exception);

                var resultModel = new ApiResult { Code = 500, Message = message };
                if (hostEnvironment.IsDevelopment())
                {
                    resultModel.Exception = context.Exception;
                }
                context.HttpContext.Response.StatusCode = 500;
                context.ExceptionHandled = true; //异常已处理
                context.Result = new JsonResult(resultModel);
            }
        }
    }
}