using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Simple.AspNetCore.Helper;
using Simple.AspNetCore.Services;
using Simple.RedisClient;
using Simple.Utils;
using Simple.Utils.Helper;
using Simple.Utils.Models.BO;
using System.Reflection;

namespace Simple.AspNetCore.Controllers
{
    /// <summary>具有鉴权的控制器，能获取到登录的用户信息,并直接拒绝无权限的访问</summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AppAuthController : Controller
    {
         
    }
}