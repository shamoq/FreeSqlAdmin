using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Entitys;
using Simple.AspNetCore.Controllers;
using Simple.EntityFrameworkCore;
using Simple.AdminApplication.DbContexts;

namespace Simple.AdminController.Controllers
{
    /// <summary>菜单控制器</summary>
    [PermissionGroup("字典Curd")]
    public class SysDicController : AppCurdController<SysDicType, SysDicType>
    {
        public SysDicController()
        {
        }
    }
}