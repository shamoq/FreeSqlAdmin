using Newtonsoft.Json;
using Simple.Utils.Exceptions;

namespace Simple.Utils.Models
{
    /// <summary>API调用结果</summary>
    public class ApiResult
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>应答数据</summary>
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("success")]
        public bool IsSuccess
        { get { return Code == 0; } }

        /// <summary>
        /// 异常信息
        /// </summary>
        public virtual Exception Exception { get; set; }

        /// <summary>生成失败时的应答实例</summary>
        /// <param name="message">应答消息</param>
        /// <returns>ApiResult 实例</returns>
        public static ApiResult Fail(string message)
        {
            throw new CustomException(message);
            // return new ApiResult { Code = 500, Message = message };
        }

        /// <summary>生成成功时的应答实例</summary>
        /// <returns>ApiResult 实例</returns>
        public static ApiResult Success()
        {
            return new ApiResult { Code = 0, Message = "success" };
        }

        /// <summary>生成成功时的应答实例</summary>
        /// <param name="data">应答数据</param>
        /// <param name="message">应答消息</param>
        /// <returns>ApiResult 实例</returns>
        public static ApiResult Success(Object data, string message = "success")
        {
            return new ApiResult { Code = 0, Data = data, Message = message };
        }
    }
}