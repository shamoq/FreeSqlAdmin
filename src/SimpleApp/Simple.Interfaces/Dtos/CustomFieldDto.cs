using Newtonsoft.Json.Linq;
using Simple.AdminApplication.TemplateMng.Enums;
using Simple.Interfaces.Dtos.FieldProp;
using Simple.Utils.Extensions;
using Simple.Utils.Helper;
using Simple.Utils.Models;
using Simple.Utils.Models.Dto;

namespace Simple.Interfaces.Dtos;

/// <summary>
/// 字段控件信息
/// </summary>
public class CustomFieldDto : InputId, ITreeDto
{
    ///  <summary>
    /// 字段编码，自动生成的
    /// </summary>
    public string Code { get; set; }

    ///  <summary>
    /// 字段编码全编码(父级编码.当前编码)
    /// </summary>
    public string FullCode { get; set; }

    /// <summary>
    /// 字段名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 控件说明
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 父级分组类型
    /// </summary>
    public string ParentGroupType { get; set; }

    /// <summary>
    /// 字段类型
    /// </summary>
    public string FieldTypeText { get; set; }

    /// <summary>
    /// 字段类型枚举
    /// </summary>
    public int FieldType { get; set; }

    /// <summary>
    /// 排序Id
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 所属父级
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 是否分组 main, sub
    /// </summary>
    public string GroupType { get; set; }

    /// <summary>
    /// 是否系统级
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 业务Id
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public DataSetFieldProp Prop { get; set; }

    public object TransformValue(Dictionary<string, object> data, CustomFieldDto parent,int index)
    {
        if (Code == "$index")
        {
            return index;
        }

        if (Code == "$now")
        {
            return DateTime.Now;
        }

        // data是嵌套父级结构
        if (parent != null)
        {
            if (data.TryGetValue(parent.Code, out var parentData))
            {
                if (parentData is Dictionary<string, object> parentDataDic)
                {
                    if (parentDataDic.TryGetValue(Code, out var fieldValue))
                    {
                        return fieldValue;
                    }
                }
            }
        }
        else
        {
            if (data.TryGetValue(Code, out var fieldValue))
            {
                return fieldValue;
            }
        }

        return string.Empty;
    }
}