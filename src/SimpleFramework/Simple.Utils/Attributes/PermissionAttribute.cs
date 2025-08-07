namespace Simple.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class PermissionAttribute : Attribute
    {
        public PermissionAttribute(string code, string name, string group = "")
        {
            Name = name;
            Code = code;
            Group = group;
        }

        /// <summary>权限组</summary>
        public string Group { get; set; }

        /// <summary>权限名称</summary>
        public string Name { get; set; }

        /// <summary>权限码</summary>
        public string Code { get; set; }
    }
}