using Simple.Interfaces.Dtos;

namespace Simple.AdminController.Models;


/// <summary>
/// 查询上下文
/// </summary>
public class SqlQueryHelperNameContext : DyamicSqlInput
{
    /// <summary>
    /// 返回的数据节点名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 查询方式 1 list  2 value
    /// </summary>
    public int QueryType { get; set; } = 1;
}