namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;

    internal static class ExpressionExtensions
    {
        public static Expression ToObject(Expression expression)
        {
            return expression.Type == typeof(object) ? expression : Expression.Convert(expression, typeof(object));
        }

        public static Expression ToType(Expression expression, Type type)
        {
            return expression.Type == type ? expression : Expression.Convert(expression, type);
        }
    }
}