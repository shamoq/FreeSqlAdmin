using Simple.AdminApplication.SysMng.Entities;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminApplication.SysMng.Controllers
{
    /// <summary>菜单控制器</summary>
    [Permission("SysDic","系统管理")]
    public class DicTypeController : AppCurdController<SysDicType, SysDicType>
    {
    }
}