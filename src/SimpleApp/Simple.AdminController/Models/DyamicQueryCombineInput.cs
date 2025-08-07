namespace Simple.AdminController.Models;

/// <summary>
/// 组合查询输入参数
/// </summary>
public class DyamicQueryCombineInput
{
    public List<SqlQueryHelperNameContext> Items { get; set; }

    public Dictionary<string, object> Parameters { get; set; }
}