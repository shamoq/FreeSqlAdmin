namespace Simple.AdminApplication.SysMng.Config
{
    public class OssConfig
    {
        /// <summary>
        /// 存储类型
        /// </summary>
        public string StoreType { get; set; }

        /// <summary>
        ///存储文件夹
        /// </summary>
        public string SaveFolder { get; set; }

        /// <summary>保存文件的前缀，默认为年月日</summary>
        public string FileFolderPrefix { get; set; }

        /// <summary>单文件最大值 bit</summary>
        public int MaxSizeLimit { get; set; }

        /// <summary>文件后缀 逗号连接 jpg,jpeg,png,exe</summary>
        public List<string> AllowExtensions { get; set; }
    }
}