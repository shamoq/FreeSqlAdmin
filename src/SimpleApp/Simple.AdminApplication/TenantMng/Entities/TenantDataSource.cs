using System.ComponentModel.DataAnnotations;

namespace Simple.AdminApplication.TenantMng.Entities;

public class TenantDataSource : DefaultNoTenantEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [StringLength(128)]
    public string Name { get; set; }
    
    /// <summary>
    /// 数据源类型
    /// </summary>
    public string DbType { get; set; }

    /// <summary>
    /// 数据源连接字符串
    /// </summary>
    public string ConnectionString { get; set; }
    
    /// <summary>
    /// 数据源连接字符串
    /// </summary>
    [StringLength(1024)]
    public string ConnectionParams { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    public string Remark { get; set; }
}