using Newtonsoft.Json;

namespace Simple.Utils.Models.Dto;

public class OptionDto
{
    [JsonProperty("value")]
    public Guid Id { get; set; }

    [JsonProperty("label")]
    public string Name { get; set; }

    /// <summary>
    /// 扩展数据
    /// </summary>
    public object ExtData { get; set; }

    public OptionDto()
    {
    }

    public OptionDto(Guid id, string name, object extData = null)
    {
        Id = id;
        Name = name;
        ExtData = extData;
    }
}