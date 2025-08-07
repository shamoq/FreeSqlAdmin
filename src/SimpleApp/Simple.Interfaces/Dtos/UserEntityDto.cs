using Simple.Utils.Models;

namespace Simple.Interfaces.Dtos;

public class UserEntityDto : InputId
{
    public string UserName { get; set; }
    public string UserCode { get; set; }
    public int IsAdmin { get; set; }
}