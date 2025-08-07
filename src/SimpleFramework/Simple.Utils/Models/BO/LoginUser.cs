using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Utils.Models.Dto;

namespace Simple.Utils.Models.BO
{
    /// <summary>
    /// 登录账户信息
    /// </summary>
    public class LoginUserBO : BaseDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public Guid? OrgId { get; set; }
        public int IsAdmin { get; set; } = 0;
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// 会话Id
        /// </summary>
        public Guid SessionId { get; set; } = Guid.NewGuid();
    }
}