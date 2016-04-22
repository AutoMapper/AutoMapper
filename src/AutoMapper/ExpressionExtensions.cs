namespace AutoMapper
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;

    internal static class ExpressionExtensions
    {
        public static Expression ToObject(Expression expression)
        {
            return expression.Type == typeof(object) ? expression : Convert(expression, typeof(object));
        }

        public static Expression ToType(Expression expression, Type type)
        {
            return expression.Type == type ? expression : Convert(expression, type);
        }

        public static Expression ConsoleWriteLine(string value, params Expression[] values)
        {
            return Call(typeof (Debug).GetMethod("WriteLine", new[] {typeof (string), typeof(object[])}), 
                Constant(value), 
                NewArrayInit(typeof(object), values.Select(ToObject).ToArray()));
        }
    }
}