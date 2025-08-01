namespace Simple.Interfaces.Dtos
{
    public class RoleUserRightDto
    {
        /// <summary>
        /// 导航编码
        /// </summary>
        public string NavCode { get; set; }

        /// <summary>
        /// 菜单编码
        /// </summary>
        public string MenuCode { get; set; }

        /// <summary>
        /// 系统编码
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// 动作点
        /// </summary>
        public string ActionCode { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }
    }
}