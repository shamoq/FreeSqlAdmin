using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Utils.Models.Dto;

public class OptionObjectDto : BaseDto
{
    [JsonProperty("value")]
    public object Id { get; set; }

    [JsonProperty("label")]
    public string Name { get; set; }

    public OptionObjectDto()
    {
    }

    public OptionObjectDto(object id, string name)
    {
        Id = id;
        Name = name;
    }
}