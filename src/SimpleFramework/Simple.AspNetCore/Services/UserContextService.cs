using Microsoft.AspNetCore.Http;
using Simple.Utils.Attributes;
using Simple.Utils.Models.BO;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.AspNetCore.Helper;

namespace Simple.AspNetCore.Services
{
    [Scoped]
    public class UserContextService
    {
        /// <summary>
        /// Singleton of IHttpContextAccessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public LoginUserBO GetUser()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items.TryGetValue("UserInfo", out var userInfoObj))
            {
                return userInfoObj as LoginUserBO;
            }

            if (httpContext != null && httpContext.User != null)
            {
                return JWTHelper.GetPayload(httpContext.User.Claims.ToArray());
            }

            return null;
        }
    }
}