using System.Reflection;
using FreeSql.Internal.Model;
using Simple.Utils.Exceptions;
using Simple.Utils.Helper;
using DateTime = System.DateTime;

namespace Simple.AdminApplication.Helpers;

public class FreeSqlFilterHelper
{
    /// <summary>
    /// 将前端的filter转换为动态查询条件
    /// </summary>
    /// <returns></returns>
    public static DynamicFilterInfo GetDynamicFilterInfo<T>(List<Filter> filters, PropertyInfo[] props = null)
    {
        if (props == null)
        {
            props = typeof(T).GetProperties();
        }

        var filterInfo = new DynamicFilterInfo();
        filterInfo.Logic = DynamicFilterLogic.Or;
        filterInfo.Filters = new List<DynamicFilterInfo>();

        if (filters == null || !filters.Any()) return filterInfo;

        filterInfo.Filters = new List<DynamicFilterInfo>();
        foreach (var filter in filters)
        {
            var subFilter = new DynamicFilterInfo();

            // 处理子级Filter
            if (filter.Filters != null && filter.Filters.Any())
            {
                subFilter = GetDynamicFilterInfo<T>(filter.Filters, props);
                subFilter.Logic = filter.Type?.ToLower() == "or"
                    ? DynamicFilterLogic.Or
                    : DynamicFilterLogic.And;
            }
            else
            {
               var prop = props.FirstOrDefault(
                    t => t.Name.Equals(filter.Field, StringComparison.OrdinalIgnoreCase));
                if (prop == null) throw new FatalException("查询字段不存在");
                // 默认运算符
                if (string.IsNullOrEmpty(filter.Op))
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        filter.Op = "like";
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?) ||
                             prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)
                             || prop.PropertyType == typeof(long) || prop.PropertyType == typeof(long?) ||
                             prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?)
                             || prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?))
                    {
                        filter.Op = "=";
                    }
                }

                subFilter.Operator = ParaseOp(filter.Op);
                subFilter.Field = filter.Field;
                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    subFilter.Value = TypeConvertHelper.ConvertType<DateTime?>(filter.Value);
                }
                else
                {
                    subFilter.Value = filter.Value;
                }
            }

            filterInfo.Filters.Add(subFilter);
        }

        return filterInfo;
    }


    /// <summary>转换操作符</summary>
    /// <param name="op"></param>
    /// <returns></returns>
    /// <exception cref="FatalException"></exception>
    private static DynamicFilterOperator ParaseOp(string op)
    {
        switch (op.ToLower())
        {
            case "=":
                return DynamicFilterOperator.Equal;

            case "like":
                return DynamicFilterOperator.Contains;
            case "llike":
                return DynamicFilterOperator.StartsWith;
            case "nllike":
                return DynamicFilterOperator.NotStartsWith;
            case "rlike":
                return DynamicFilterOperator.EndsWith;
            case "nlike":
                return DynamicFilterOperator.NotContains;

            case ">":
                return DynamicFilterOperator.GreaterThan;

            case ">=":
                return DynamicFilterOperator.GreaterThanOrEqual;

            case "!=":
                return DynamicFilterOperator.NotEqual;

            case "<":
                return DynamicFilterOperator.LessThan;

            case "<=":
                return DynamicFilterOperator.LessThanOrEqual;

            case "in":
                return DynamicFilterOperator.Any;

            case "notin":
                return DynamicFilterOperator.NotAny;

            case "range":
                return DynamicFilterOperator.Range;

            case "daterange":
                return DynamicFilterOperator.DateRange;

            default:
                throw new Exception("操作类型 Op 未定义");
        }
    }
}