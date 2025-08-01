namespace Simple.Utils.Attributes;

// 用于标记需要生成 TypeScript 的类
[AttributeUsage(AttributeTargets.Class)]
public class GenerateTypeScriptAttribute : Attribute
{
    public string Namespace { get; set; }
    
    public GenerateTypeScriptAttribute(string @namespace = null)
    {
        Namespace = @namespace;
    }
}