using Simple.Utils.Models.Dto;

namespace Simple.Interfaces.Dtos.FieldProp;

public class ImageProp : BaseDto
{
    /// <summary>
    /// 图片宽度
    /// </summary>
    public int Width { get; set; } = 100;

    /// <summary>
    /// 图片高度
    /// </summary>
    public int Height { get; set; } = 100;

    /// <summary>
    /// 图片类型 ImageFieldTypeEnum
    /// </summary>
    public string ImageType { get; set; }

    public string TransformValue(object value)
    {
        return value?.ToString();
    }
}