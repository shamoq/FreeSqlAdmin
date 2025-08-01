using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Simple.Utils.Attributes;
using Simple.Utils.Exceptions;
using Simple.Utils.Extensions;

namespace Simple.Utils.Helper
{
    /// <summary>操作枚举</summary>
    public enum OpEnum
    {
        /// <summary>!= 不等于</summary>
        notequal,

        /// <summary>= 等于</summary>
        equal,

        /// <summary>包含 like</summary>
        contains,

        /// <summary>大于 &gt;</summary>
        greater,

        /// <summary>大于等于 &gt;=</summary>
        greaterorequal,

        /// <summary>/* 小于 */</summary>
        less,

        /// <summary>/* 小于等于 */</summary>
        lessorequal,

        /// <summary>在 、、之中</summary>
        In,

        /// <summary>在 、、之间</summary>
        between
    }

    /// <summary>条件查询表达式的扩展</summary>
    internal class QueryExpression<T>
    {
        private ParameterExpression parameter;

        public QueryExpression()
        {
            parameter = Expression.Parameter(typeof(T));
        }

        private Expression ParseExpressionBody(List<Filter> conditions, string type = "or")
        {
            if (conditions == null || conditions.Count() == 0)
            {
                return Expression.Constant(true, typeof(bool));
            }

            Expression lasestExpress = null;
            foreach (var condition in conditions)
            {
                Expression express = null;
                // 有嵌套条件
                if (!string.IsNullOrEmpty(condition.Type) &&
                    string.IsNullOrEmpty(condition.Field) &&
                    condition.Filters != null)
                {
                    if (condition.Filters.Any())
                    {
                        express = ParseExpressionBody(condition.Filters, condition.Type);
                        if (express == null)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // 嵌套条件下又没有任何条件，不能拼接
                        continue;
                    }
                }
                else if (!string.IsNullOrEmpty(condition.Field) && !string.IsNullOrEmpty(condition.Value)) // 字段作为条件，必须要有值
                {
                    var filter = new Filter
                    {
                        Field = condition.Field,
                        Type = condition.Type,
                        Op = condition.Op,
                        Value = condition.Value
                    };
                    express = ParseCondition(filter);
                }
                else
                {
                    continue;
                }

                // 表达式拼接
                if (lasestExpress is null)
                {
                    lasestExpress = express;
                }
                else
                {
                    lasestExpress = type == "and" ? Expression.And(lasestExpress, express) : Expression.Or(lasestExpress, express);
                }
            }

            // var expresses = new List<FiterExpress>();
            //conditions.ForEach(condition =>
            //{
            //    var filter = new FiterExpress
            //    {
            //        field = condition.field,
            //        isAnd = condition.isAnd,
            //        op = condition.op,
            //        value = condition.value
            //    };
            //    var express = ParseCondition(filter);
            //    filter.Expression = express;
            //    expresses.Add(filter);
            //});

            //Expression lasestExpress = null;
            //for (int i = 1; i < expresses.Count; i++)
            //{
            //    if (lasestExpress is null)
            //        lasestExpress = expresses[i - 1].Expression;

            //    var now = expresses[i];
            //    lasestExpress = now.isAnd ? Expression.And(lasestExpress, now.Expression)
            //        : Expression.Or(lasestExpress, now.Expression);
            //}
            return lasestExpress;
        }

        private Expression ParseCondition(Filter condition)
        {
            ParameterExpression p = parameter;
            Expression key = null;

            try
            {
                key = Expression.Property(p, condition.Field.ToPascalCase());
            }
            catch (Exception ex)
            {
                throw new FatalException($"模型{p.Type}中不存在字段{condition.Field}", ex);
            }

            // 条件默认值
            if (string.IsNullOrEmpty(condition.Op))
            {

                if (key.Type == typeof(string))
                {
                    condition.Op = "like";
                }
                else if (key.Type == typeof(int) || key.Type == typeof(int?) || key.Type == typeof(decimal) || key.Type == typeof(decimal?)
                    || key.Type == typeof(long) || key.Type == typeof(long?) || key.Type == typeof(double) || key.Type == typeof(double?)
                    || key.Type == typeof(float) || key.Type == typeof(float?))
                {
                    condition.Op = "=";
                }
            }

            var opEnum = ParaseOp(condition.Op);

            object convertValue = condition.Value;
            if (opEnum != OpEnum.In || opEnum != OpEnum.between)
            {
                convertValue = TypeConvertHelper.ConvertType(condition.Value, key.Type);
            }
            Expression value = Expression.Constant(convertValue);

            switch (opEnum)
            {
                case OpEnum.contains:
                    // Expression stringValue = Expression.Constant(convertValue.ToString());
                    return Expression.Call(key, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), value);

                case OpEnum.equal:
                    return Expression.Equal(key, Expression.Convert(value, key.Type));

                case OpEnum.greater:
                    return Expression.GreaterThan(key, Expression.Convert(value, key.Type));

                case OpEnum.greaterorequal:
                    return Expression.GreaterThanOrEqual(key, Expression.Convert(value, key.Type));

                case OpEnum.less:
                    return Expression.LessThan(key, Expression.Convert(value, key.Type));

                case OpEnum.lessorequal:
                    return Expression.LessThanOrEqual(key, Expression.Convert(value, key.Type));

                case OpEnum.notequal:
                    return Expression.NotEqual(key, Expression.Convert(value, key.Type));

                case OpEnum.In:
                    return ParaseIn(condition);

                case OpEnum.between:
                    return ParaseBetween(condition);

                default:
                    throw new NotImplementedException("不支持此操作");
            }
        }

        /// <summary>Between 转换</summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private Expression ParaseBetween(Filter conditions)
        {
            ParameterExpression p = parameter;
            var value = conditions.Value.ToString();
            Expression key = Expression.Property(p, conditions.Field);
            var valueArr = value.Split(',');
            if (valueArr.Length != 2)
            {
                throw new NotImplementedException("ParaseBetween参数错误");
            }
            try
            {
                int.Parse(valueArr[0]);
                int.Parse(valueArr[1]);
            }
            catch
            {
                throw new NotImplementedException("ParaseBetween参数只能为数字");
            }

            //开始位置
            Expression startvalue = Expression.Constant(int.Parse(valueArr[0]));
            Expression start = Expression.GreaterThanOrEqual(key, Expression.Convert(startvalue, key.Type));

            Expression endvalue = Expression.Constant(int.Parse(valueArr[1]));
            Expression end = Expression.LessThanOrEqual(key, Expression.Convert(endvalue, key.Type));
            return Expression.AndAlso(start, end);
        }

        /// <summary>In 转换 多个值之间 ， 间隔</summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private Expression ParaseIn(Filter conditions)
        {
            ParameterExpression p = parameter;
            var fieldvalue = conditions.Value.ToString();
            Expression key = Expression.Property(p, conditions.Field);
            var valueArr = fieldvalue.Split(',');
            Expression expression = Expression.Constant(true, typeof(bool));
            foreach (var itemVal in valueArr)
            {
                Expression value = Expression.Constant(itemVal);
                Expression right = Expression.Equal(key, Expression.Convert(value, key.Type));
                expression = Expression.Or(expression, right);
            }
            return expression;
        }

        /// <summary>转换操作符</summary>
        /// <param name="op"></param>
        /// <returns></returns>
        /// <exception cref="FatalException"></exception>
        private OpEnum ParaseOp(string op)
        {
            switch (op.ToLower())
            {
                case "=":
                    return OpEnum.equal;

                case "like":
                    return OpEnum.contains;

                case ">":
                    return OpEnum.greater;

                case ">=":
                    return OpEnum.greaterorequal;

                case "!=":
                    return OpEnum.notequal;

                case "<":
                    return OpEnum.less;

                case "<=":
                    return OpEnum.lessorequal;

                case "in":
                    return OpEnum.In;

                case "<between":
                    return OpEnum.between;

                default:
                    throw new FatalException("操作类型 Op 未定义");
            }
        }

        /// <summary>根据Filter获取查询表达式</summary>
        /// <returns></returns>
        public Expression<Func<T, bool>> ParserWhere(List<Filter> filters)
        {
            //将条件转化成表达是的Body
            var query = ParseExpressionBody(filters);
            if (query == null)
            {
                return null;
            }

            return Expression.Lambda<Func<T, bool>>(query, parameter);
        }
    }

    public static class FiterExpressHelper
    {
        /// <summary>获取条件表达式</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetWhere<T>(List<Filter> filters)
        {
            var param = new QueryExpression<T>();
            return param.ParserWhere(filters);
        }
    }

    /// <summary>查询条件 And Or 查询 不支持嵌套</summary>
    [GenerateTypeScript]
    public class Filter : IEqualityComparer<Filter>
    {
        /// <summary>And 连接 默认</summary>
        public string Type { get; set; }

        /// <summary>字段名称</summary>
        public string Field { get; set; }

        /// <summary>
        /// 操作符
        /// </summary>
        /// <![CDATA[
        /// like 、 = 、！= 、> 、< 、>= 、<= 、in 、 between 
        /// ]]>
        public string Op { get; set; }

        /// <summary>值</summary>
        public string Value { get; set; }

        /// <summary>
        /// 表达式
        /// </summary>
        public List<Filter> Filters { get; set; }

        public bool Equals(Filter me, Filter other)
        {
            var result = me.Field == other.Field
                && me.Value.ToString() == other.Value.ToString()
                && me.Op == other.Op
                && me.Type == other.Type;
            return result;
        }

        public int GetHashCode(Filter me)
        {
            return me.ToString().GetHashCode();
        }
    }


}