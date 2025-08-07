﻿using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Simple.Utils.Helper
{
    /// <summary>http请求类</summary>
    public class HttpHelper
    {
        private readonly HttpClient _httpClient;
        private string _baseIPAddress;

        /// <param name="ipaddress">请求的基础IP，例如：http://192.168.1.22:8081/</param>
        /// <param name="hideHttps">取消https验证</param>
        public HttpHelper(string ipaddress = "", bool hideHttps = false)
        {
            this._baseIPAddress = ipaddress;

            if (hideHttps)
            {
                var handler = new HttpClientHandler();
                // 忽略证书验证
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                _httpClient = new HttpClient(handler) { BaseAddress = new Uri(_baseIPAddress) };
            }
            else
            {
                _httpClient = new HttpClient { BaseAddress = new Uri(_baseIPAddress) };
            }
        }

        /// <summary>创建带用户信息的请求客户端</summary>
        /// <param name="userName">用户账号</param>
        /// <param name="pwd">用户密码，当WebApi端不要求密码验证时，可传空串</param>
        /// <param name="uriString">The URI string.</param>
        /// <param name="hideHttps">取消https验证</param>
        public HttpHelper(string userName, string pwd = "", string uriString = "", bool hideHttps = false)
            : this(uriString, hideHttps)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                _httpClient.DefaultRequestHeaders.Authorization = CreateBasicCredentials(userName, pwd);
            }
        }

        /// <summary></summary>
        /// <param name="requestUrl"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private string Post(string requestUrl, HttpContent content)
        {
            var result = _httpClient.PostAsync(ConcatURL(requestUrl), content);
            Byte[] resultBytes = result.Result.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(resultBytes);
            //return result.Result.Content.ReadAsStringAsync().Result;
        }

        /// <summary>把请求的URL相对路径组合成绝对路径</summary>
        private string ConcatURL(string requestUrl)
        {
            return new Uri(_httpClient.BaseAddress, requestUrl).OriginalString;
        }

        /// <summary></summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private AuthenticationHeaderValue CreateBasicCredentials(string userName, string password)
        {
            string toEncode = userName + ":" + password;
            Encoding encoding = Encoding.GetEncoding("utf-8");
            byte[] toBase64 = encoding.GetBytes(toEncode);
            string parameter = Convert.ToBase64String(toBase64);

            return new AuthenticationHeaderValue("Basic", parameter);
        }

        /// <summary>添加header</summary>
        /// <param name="headers"></param>
        public void AddHeader(Dictionary<string, string> headers)
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        /// <summary>
        /// Get请求数据 返回字符串
        /// <para>最终以url参数的方式提交</para>
        /// </summary>
        /// <param name="parameters">参数字典,可为空</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <returns></returns>
        public string Get(Dictionary<string, string> parameters, string requestUri)
        {
            if (parameters != null)
            {
                var strParam = string.Join("&", parameters.Select(o => o.Key + "=" + o.Value));
                requestUri = string.Concat(ConcatURL(requestUri), '?', strParam);
            }
            else
            {
                requestUri = ConcatURL(requestUri);
            }

            var result = _httpClient.GetStringAsync(requestUri);
            return result.Result;
        }

        /// <summary>
        /// Get请求数据 返回数据流
        /// <para>最终以url参数的方式提交</para>
        /// </summary>
        /// <param name="parameters">参数字典,可为空</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <returns></returns>
        public System.IO.Stream GetStream(Dictionary<string, string> parameters, string requestUri)
        {
            if (parameters != null)
            {
                var strParam = string.Join("&", parameters.Select(o => o.Key + "=" + o.Value));
                requestUri = string.Concat(ConcatURL(requestUri), '?', strParam);
            }
            else
            {
                requestUri = ConcatURL(requestUri);
            }

            var result = _httpClient.GetStreamAsync(requestUri);
            return result.Result;
        }

        /// <summary>
        /// Get请求数据 返回序列化后的对象
        /// <para>最终以url参数的方式提交</para>
        /// </summary>
        /// <param name="parameters">参数字典</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <returns>实体对象</returns>
        public T Get<T>(Dictionary<string, string> parameters, string requestUri) where T : class
        {
            string jsonString = Get(parameters, requestUri);
            if (string.IsNullOrEmpty(jsonString))
                return null;

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>同步GET请求</summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="timeout">请求响应超时时间，单位/s(默认100秒)</param>
        /// <returns></returns>
        public string HttpGet(string url, Dictionary<string, string> headers = null, int timeout = 0)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            if (timeout > 0)
            {
                _httpClient.Timeout = new TimeSpan(0, 0, timeout);
            }
            var result = _httpClient.GetAsync(ConcatURL(url));
            Byte[] resultBytes = result.Result.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(resultBytes);
        }

        /// <summary>异步GET请求</summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="timeout">请求响应超时时间，单位/s(默认100秒)</param>
        /// <returns></returns>
        public async Task<string> HttpGetAsync(string url, Dictionary<string, string> headers = null, int timeout = 0)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            if (timeout > 0)
            {
                _httpClient.Timeout = new TimeSpan(0, 0, timeout);
            }
            //_httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
            var result = await _httpClient.GetAsync(ConcatURL(url));
            Byte[] resultBytes = result.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(resultBytes);
        }

        /// <summary>
        /// 以json的方式Post数据 返回string类型
        /// <para>最终以json的方式放置在http体中</para>
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <param name="timeout">请求响应超时时间，单位/s(默认100秒)</param>
        /// <param name="encoding">默认UTF8</param>
        /// <returns></returns>
        public string Post(object entity, string requestUri, Dictionary<string, string> headers = null, string contentType = "application/json", int timeout = 0, Encoding encoding = null)
        {
            string request = string.Empty;
            if (entity != null)
                request = JsonConvert.SerializeObject(entity);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            if (timeout > 0)
            {
                _httpClient.Timeout = new TimeSpan(0, 0, timeout);
            }
            HttpContent httpContent = new StringContent(request ?? "", encoding ?? Encoding.UTF8);
            if (contentType != null)
            {
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            }
            return Post(requestUri, httpContent);
        }

        /// <summary>
        /// Post Dic数据
        /// <para>最终以formurlencode的方式放置在http体中</para>
        /// </summary>
        /// <returns>System.String.</returns>
        public string PostDic(Dictionary<string, string> temp, string requestUri)
        {
            HttpContent httpContent = new FormUrlEncodedContent(temp);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return Post(requestUri, httpContent);
        }

        /// <summary></summary>
        /// <param name="bytes"></param>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public string PostByte(byte[] bytes, string requestUrl)
        {
            HttpContent content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return Post(requestUrl, content);
        }
    }
}