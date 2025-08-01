namespace Simple.Utils.Models.Dto;

/// <summary>
/// 排序入参
/// </summary>
public class SortInput
{
    /// <summary>字段</summary>
    public string Field { get; set; }

    /// <summary>排序类型</summary>
    public string Sort { get; set; } = "ASC";
}