using Simple.Utils.Comment;
using Simple.Utils.Extensions;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.Helpers;

public class TypeScriptGenerator
{
    private readonly string _outputPath;
    private readonly HashSet<Type> _processedTypes = new HashSet<Type>();
    private readonly Type _targetAttributeType = typeof(GenerateTypeScriptAttribute);

    public TypeScriptGenerator(string outputPath)
    {
        _outputPath = outputPath;
    }

    private void DeleteDir(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public void GenerateTypeScript()
    {
        // 删除现有文件夹
        DeleteDir(Path.Combine(_outputPath, "dto"));
        DeleteDir(Path.Combine(_outputPath, "enum"));
        DeleteDir(Path.Combine(_outputPath, "entity"));

        DateTime start = DateTime.Now;
        var allTypes = RuntimeHelper.GetAllTypes();

        var targetTypes = allTypes
            .Where(t => t.GetCustomAttributes(_targetAttributeType, true).Length > 0)
            .ToList();

        var test = targetTypes.Where(t => t.FullName.Contains("Contract")).ToList();

        foreach (var type in targetTypes)
        {
            GenerateTypeScriptForType(type);
        }

        Console.WriteLine($"ts生成完成，耗时:{(DateTime.Now - start).TotalSeconds}s");
    }

    private void GenerateTypeScriptForType(Type type)
    {
        if (_processedTypes.Contains(type))
            return;

        _processedTypes.Add(type);

        // 泛型类不生成
        if (type.IsGenericType)
            return;

        var directory = _outputPath;
        if (typeof(DefaultEntity).IsAssignableFrom(type))
        {
            directory = Path.Combine(_outputPath, "entity");
        }
        else if (type.IsClass && type.IsAbstract && type.IsSealed)
        {
            directory = Path.Combine(_outputPath, "enum");
        }
        else
        {
            directory = Path.Combine(_outputPath, "dto");
        }

        Directory.CreateDirectory(directory);

        var typeScriptAttribute = type.GetCustomAttribute<GenerateTypeScriptAttribute>();

        if (!string.IsNullOrEmpty(typeScriptAttribute.Namespace))
        {
            var namespaceDirectory = typeScriptAttribute.Namespace.Replace(".", Path.DirectorySeparatorChar.ToString());
            directory = Path.Combine(_outputPath, namespaceDirectory);
            Directory.CreateDirectory(directory);
        }

        var filePath = Path.Combine(directory, $"{type.Name}.ts");

        try
        {
            var tsDefinition = GenerateTypeScriptDefinition(type)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            // 使用UTF-8无BOM编码写入文件
            File.WriteAllText(filePath, tsDefinition, new UTF8Encoding(false));
            Console.WriteLine($"Generated: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating {filePath}: {ex.Message}");
        }
    }

    private string GenerateTypeScriptDefinition(Type type)
    {
        var sb = new StringBuilder();
        var typeScriptAttribute = type.GetCustomAttribute<GenerateTypeScriptAttribute>();

        if (!string.IsNullOrEmpty(typeScriptAttribute.Namespace))
        {
            sb.AppendLine($"// From C# namespace: {type.Namespace}");
            sb.AppendLine($"// TypeScript namespace: {typeScriptAttribute.Namespace}");
            sb.AppendLine();
        }

        if (type.IsEnum)
        {
            return GenerateEnumDefinition(type);
        }

        if (type.IsClass && type.IsAbstract && type.IsSealed)
        {
            return GenerateStaticClassDefinition(type);
        }

        if (type.IsClass)
        {
            return GenerateClassDefinition(type);
        }

        return $"// Unknown type: {type.Name}";
    }

    private string GenerateStaticClassDefinition(Type type)
    {
        var sb = new StringBuilder();

        // 生成枚举定义
        sb.AppendLine($"export enum {type.Name} {{");

        // 获取所有公共静态字段并按值排序
        // 获取所有公共静态字段并按值排序
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(f =>
            {
                var value = f.GetValue(null);
                return value is string ? value.ToString() : Convert.ToInt32(value).ToString();
            })
            .ToList();

        var isString = fields.All(x => x.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = field.GetValue(null);
            var displayNameAttr = field.GetCustomAttribute<DescriptionAttribute>();
            var label = displayNameAttr?.Description ?? field.Name;
            var formatValue = value is string ? $"'{EscapeString(value.ToString())}'" : value.ToString();

            // 添加注释
            sb.AppendLine($"  {field.Name} = {formatValue},    // {label}");
        }

        sb.AppendLine("}");
        sb.AppendLine();

        // 生成选项数组
        sb.AppendLine($"export const {type.Name}Option = [");
        foreach (var field in fields)
        {
            var value = field.GetValue(null);
            var displayNameAttr = field.GetCustomAttribute<DescriptionAttribute>();
            var label = displayNameAttr?.Description ?? field.Name;
            var formatValue = value is string ? $"'{EscapeString(value.ToString())}'" : value.ToString();

            sb.AppendLine($"  {{ label: '{EscapeString(label)}', value: {formatValue} }},");
        }

        sb.AppendLine("];");
        sb.AppendLine();

        // 生成获取标签的辅助函数
        var enumType = isString ? "string" : "number";
        sb.AppendLine(
            $"export function get{type.Name.Replace("Enum", "")}Label(value: {enumType}): string | undefined {{");
        sb.AppendLine($"  const item = {type.Name}Option.find(x => x.value === value);");
        sb.AppendLine("  return item?.label;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateClassDefinition(Type type)
    {
        var bodyCode = new StringBuilder();

        // 收集需要导入的类型
        var importTypes = new HashSet<string>();
        foreach (var property in GetSerializableProperties(type))
        {
            var propertyType = property.PropertyType;

            if (propertyType.IsInterface)
            {
                continue;
            }

            if (propertyType.IsGenericType)
            {
                foreach (var genericArg in propertyType.GetGenericArguments())
                {
                    if (!IsBasicType(genericArg) && genericArg.Name != type.Name)
                    {
                        importTypes.Add(genericArg.Name);
                    }
                }
            }
            else if (!IsBasicType(propertyType))
            {
                importTypes.Add(propertyType.Name);
            }
        }

        // 添加导入语句
        if (importTypes.Any())
        {
            foreach (var importType in importTypes.OrderBy(x => x))
            {
                bodyCode.AppendLine($"import type {{ {importType} }} from './{importType}';");
            }

            bodyCode.AppendLine();
        }

        bodyCode.AppendLine($"export interface {type.Name} {{");

        foreach (var property in GetSerializableProperties(type))
        {
            if (property.PropertyType.IsInterface)
            {
                continue;
            }

            var dyamicProp = property.GetCustomAttribute<Newtonsoft.Json.JsonExtensionDataAttribute>();
            if (dyamicProp != null)
            {
                continue;
            }

            var propertyType = GetTypeScriptType(property.PropertyType);
            var propertyName = GetPropertyName(property);
            var propertyNameCase = propertyName.ToCamelCase();
            var isOptional = IsOptionalProperty(property);
            var desc = CSCommentReader.Create(property)?.Summary;
            if (!string.IsNullOrEmpty(desc))
            {
                desc = "    //  " + desc;
            }

            // 强制集合不能为空
            if (propertyType.Contains("[]"))
            {
                isOptional = false;
            }
            // else if (propertyType.Contains("{"))
            // {
            //     isOptional = false;
            // }
            // else
            // {
            //     propertyType = propertyType + (isOptional? " | null" : "");
            // }

            // bodyCode.AppendLine($"  {propertyNameCase}{(isOptional ? "?" : "")}: {propertyType};" + desc);
            bodyCode.AppendLine($"  {propertyNameCase}: {propertyType} {(isOptional ? " | null | undefined " : "")};" +
                                desc);
        }

        bodyCode.AppendLine("}");
        return bodyCode.ToString();
    }

    private bool IsBasicType(Type type)
    {
        if (type == typeof(string) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(short) ||
            type == typeof(byte) ||
            type == typeof(double) ||
            type == typeof(float) ||
            type == typeof(decimal) ||
            type == typeof(bool) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(Guid) ||
            type == typeof(object))
        {
            return true;
        }

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(Dictionary<,>))
            {
                return true;
            }
        }

        return false;
    }

    private string GenerateEnumDefinition(Type type)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"export enum {type.Name} {{");

        foreach (var name in Enum.GetNames(type))
        {
            var value = Enum.Parse(type, name);
            var underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(type));
            var fieldInfo = type.GetField(name);
            var labelAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            var label = labelAttr?.Description ?? name;

            sb.AppendLine($"  {name} = {underlyingValue},");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private IEnumerable<PropertyInfo> GetSerializableProperties(Type type)
    {
        return type.GetProperties()
            .Where(p => p.GetMethod != null && p.GetIndexParameters().Length == 0);
    }

    private string GetPropertyName(PropertyInfo property)
    {
        var jsonPropertyAttribute = property.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "JsonPropertyAttribute");

        if (jsonPropertyAttribute != null)
        {
            var propertyName =
                jsonPropertyAttribute.GetType().GetProperty("PropertyName")?.GetValue(jsonPropertyAttribute) as string;
            if (!string.IsNullOrEmpty(propertyName))
                return propertyName;
        }

        return property.Name;
    }

    private bool IsOptionalProperty(PropertyInfo property)
    {
        if (property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            return true;

        var requiredAttribute = property.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "RequiredAttribute");

        return requiredAttribute == null;
    }

    private string GetTypeScriptType(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            return "number";
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) return "string";
        if (type == typeof(Guid)) return "string";
        if (type == typeof(object)) return "any";

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = type.GetGenericArguments()[0];
            return GetTypeScriptType(underlyingType);
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return $"{GetTypeScriptType(elementType)}[]";
        }

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(List<>) ||
                genericType == typeof(IEnumerable<>) ||
                genericType == typeof(ICollection<>) ||
                genericType == typeof(IList<>))
            {
                var elementType = type.GetGenericArguments()[0];
                return $"{GetTypeScriptType(elementType)}[]";
            }

            if (genericType == typeof(Dictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                return $"{{ [key: {GetTypeScriptType(keyType)}]: {GetTypeScriptType(valueType)} }}";
            }
        }

        if (type.IsEnum)
        {
            return type.Name;
        }

        return type.Name;
    }

    private string EscapeString(string value)
    {
        return value.Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}