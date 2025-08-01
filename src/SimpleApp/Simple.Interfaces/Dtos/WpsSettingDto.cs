namespace Simple.Interfaces.Dtos
{
    public class WpsSettingDto
    {
        /// <summary>
        ///     授权ID
        /// </summary>
        public virtual string AppId { get; set; }

        /// <summary>
        ///     授权Key
        /// </summary>
        public virtual string AppKey { get; set; }

        /// <summary>
        ///     回调网址
        /// </summary>
        public virtual string CallbackUrl { get; set; }

        /// <summary>
        /// 回调Id
        /// </summary>
        public virtual string CallbackUser { get; set; }

        ///<summary>
        /// 文件唯一Key
        ///</summary>
        public virtual string FileDocKey { get; set; }
    }
}