using System.Linq.Expressions;
using Simple.Utils.Attributes;
using Simple.Utils.Extensions;
using Simple.Utils.Helper;
using Simple.Utils.Models.Dto;

namespace Simple.Utils.Models
{
    /// <summary>分页请求</summary>
    public class QueryRequestInput : BaseDto
    {
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 1;
        public List<Filter> Filters { get; set; } = new List<Filter>();

        public string SortField { get; set; } // 排序字段
        public string SortType { get; set; }  // 排序方向

        // 存储表达式的属性，使用接口类型
        public IExpressionHolder AdditionalExpression { get; set; }
    }

    // 定义一个接口，用于封装表达式
    public interface IExpressionHolder
    {
        LambdaExpression InnerExpression { get; }
    }

    public class ExpressionHolder<T> : IExpressionHolder
    {
        private Expression<Func<T, bool>> query = null;

        public LambdaExpression InnerExpression
        {
            get { return query; }
        }

        public ExpressionHolder<T> AddAnd(Expression<Func<T, bool>> expression)
        {
            if (query == null)
            {
                query = expression;
            }
            else
            {
                query = query.And(expression);
            }

            return this;
        }

        public ExpressionHolder<T> AddOr(Expression<Func<T, bool>> expression)
        {
            if (query == null)
            {
                query = expression;
            }
            else
            {
                query = query.Or(expression);
            }

            return this;
        }


        public ExpressionHolder<T> AddIf(bool result, Expression<Func<T, bool>> expression)
        {
            if (!result)
            {
                return this;
            }

            return AddAnd(expression);
        }

        public ExpressionHolder<T> AddOr(bool result, Expression<Func<T, bool>> expression)
        {
            if (!result)
            {
                return this;
            }

            return AddOr(expression);
        }

        public ExpressionHolder<T> AddIf(bool? result, Expression<Func<T, bool>> expression)
        {
            return AddIf(result != null && result.Value, expression);
        }

        public ExpressionHolder<T> AddOr(bool? result, Expression<Func<T, bool>> expression)
        {
            return AddOr(result != null && result.Value, expression);
        }

        public Expression<Func<T, bool>> ToExpression()
        {
            return query;
        }
    }
}