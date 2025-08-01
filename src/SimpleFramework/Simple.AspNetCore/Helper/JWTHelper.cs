using Microsoft.IdentityModel.Tokens;
using Simple.AspNetCore.Models;
using Simple.Utils.Helper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Simple.Utils.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using Simple.Utils.Models.BO;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Simple.AspNetCore.Helper
{
    /// <summary>jwt 帮助类</summary>
    public class JWTHelper
    {
        /// <summary>JWT配置</summary>
        private static readonly JwtSetting settings;

        /// <summary>构造函数</summary>
        /// <param name="settings"></param>
        static JWTHelper()
        {
            settings = ConfigHelper.GetValue<JwtSetting>("Jwt");
        }

        /// <summary>系统登录，生成Jwt</summary>
        /// <returns></returns>
        public static string CreateToken(LoginUserBO payload)
        {
            var claims = GetClaims(payload);
            return CreateToken(claims);
        }

        /// <summary>刷新token</summary>
        /// <returns></returns>
        public static string RefreshToken(string oldToken)
        {
            // 使用JsonWebToken读取oldToken，并重新更换时间
            var handler = new JsonWebTokenHandler();
            var token = handler.ReadJsonWebToken(oldToken.Replace("Bearer ", ""));
            // 重新生成新的token
            var newToken = CreateToken(token.Claims);
            return newToken;
        }

        /// <summary>生成Jwt</summary>
        /// <param name="claims"></param>
        /// <param name="Minutes">过期时间</param>
        /// <returns></returns>
        private static string CreateToken(IEnumerable<Claim> claims, int Minutes = 0)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = settings.Issuer,
                Audience = settings.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = Minutes > 0 ? DateTime.Now.AddMinutes(Minutes) : DateTime.Now.AddMinutes(settings.ExpMinutes),
                SigningCredentials = creds
            };

            var handler = new JsonWebTokenHandler();
            var token = handler.CreateToken(tokenDescriptor); // 使用 JsonWebTokenHandler 生成 Token

            HostServiceExtension.CurrentHttpContext?.Session.Set(ConfigHelper.GetValue("TokenHeadKey"), token);
            return "Bearer " + token;
        }   

        /// <summary>从Token中获取用户身份</summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static ClaimsPrincipal GetPrincipal(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                return handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecurityKey)),
                    ValidateLifetime = false
                }, out SecurityToken validatedToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>校验Token</summary>
        /// <param name="token">token</param>
        /// <returns></returns>
        public static bool CheckToken(string token)
        {
            var principal = GetPrincipal(token);
            if (principal is null)
            {
                return false;
            }

            return true;
        }

        private static Claim[] GetClaims(LoginUserBO payload)
        {
            //声明claim
            var claims = new Claim[]
            {
                new Claim("c", payload.UserCode),
                new Claim("n", payload.UserName),
                new Claim("id", payload.Id.ToString()),
                new Claim("iat", DateTime.UtcNow.ToUnix().ToString(), ClaimValueTypes.Integer64), //签发时间
                new Claim("nbf", DateTime.UtcNow.ToUnix().ToString(), ClaimValueTypes.Integer64), //生效时间
                new Claim("iss", settings.Issuer),
                new Claim("aud", settings.Audience),
                new Claim("org", payload.OrgId?.ToString() ?? ""),
                new Claim("a", payload.IsAdmin.ToString()),
                new Claim("t", payload.TenantId.ToString()),
                new Claim("s", payload.SessionId.ToString()),
            };

            return claims;
        }

        /// <summary>
        /// 从载荷中获取用户信息
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static LoginUserBO GetPayload(Claim[] claims)
        {
            var jwtPayloadInfo = new LoginUserBO();

            foreach (var claim in claims)
            {
                switch (claim.Type)
                {
                    case "c":
                        jwtPayloadInfo.UserCode = claim.Value;
                        break;

                    case "n":
                        jwtPayloadInfo.UserName = claim.Value;
                        break;

                    case "id":
                        jwtPayloadInfo.Id = Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
                        break;
                    
                    case "s":
                        jwtPayloadInfo.SessionId = Guid.TryParse(claim.Value, out var sessionId) ? sessionId : Guid.Empty;
                        break;

                    case "org":
                        jwtPayloadInfo.OrgId = Guid.TryParse(claim.Value, out var id3) ? id3 : null;
                        break;

                    case "a":
                        jwtPayloadInfo.IsAdmin = int.Parse(claim.Value);
                        break;

                    case "t":
                        jwtPayloadInfo.TenantId = Guid.TryParse(claim.Value, out var guid4)? guid4: Guid.Empty;
                        break;
                }
            }

            return jwtPayloadInfo;
        }
    }

    ///// <summary>从token中获取用户身份</summary>
    ///// <param name="token"></param>
    ///// <returns></returns>
    //public static IEnumerable<Claim> GetClaims(string token)
    //{
    //    var handler = new JwtSecurityTokenHandler();
    //    var securityToken = handler.ReadJwtToken(token);
    //    return securityToken?.Claims;
    //}
}