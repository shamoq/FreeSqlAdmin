namespace Simple.Utils.Attributes
{
    /// <summary>自动注册每次实例化</summary>
    public class TransientAttribute : Attribute
    { }

    /// <summary>自动注册请求内单例</summary>
    public class ScopedAttribute : Attribute
    { }

    /// <summary>自动注册全局单例</summary>
    public class SingletonAttribute : Attribute
    { }
}