namespace Simple.Utils.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EnumColumnAttribute: Attribute
{
    public Type EnumType { get; private set; }
    
    public string TextColumnName { get; private set; }

    public EnumColumnAttribute(Type enumType, string textColumnName)
    {
        EnumType = enumType;
        TextColumnName = textColumnName;
    }
}