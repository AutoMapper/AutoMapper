using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    using static Expression;

    internal static class ExpressionExtensions
    {
        public static Expression MemberAccesses(this IEnumerable<MemberInfo> members, Expression obj) =>
            members.Aggregate(obj, (expression, member) => MakeMemberAccess(expression, member));

        public static IEnumerable<MemberExpression> GetMembers(this Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if(memberExpression == null)
            {
                return new MemberExpression[0];
            }
            return memberExpression.GetMembers();
        }

        public static IEnumerable<MemberExpression> GetMembers(this MemberExpression expression)
        {
            while(expression != null)
            {
                yield return expression;
                expression = expression.Expression as MemberExpression;
            }
        }

        public static bool IsMemberPath(this LambdaExpression exp)
        {
            return exp.Body.GetMembers().LastOrDefault()?.Expression == exp.Parameters.First();
        }

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
            => ExpressionFactory.ReplaceParameters(exp, replace);

        public static Expression ConvertReplaceParameters(this LambdaExpression exp, params Expression[] replace)
            => ExpressionFactory.ConvertReplaceParameters(exp, replace);

        public static Expression Replace(this Expression exp, Expression old, Expression replace)
            => ExpressionFactory.Replace(exp, old, replace);

        public static LambdaExpression Concat(this LambdaExpression expr, LambdaExpression concat)
            => ExpressionFactory.Concat(expr, concat);

        public static Expression NullCheck(this Expression expression, Type destinationType)
            => ExpressionFactory.NullCheck(expression, destinationType);

        public static Expression IfNullElse(this Expression expression, Expression then, Expression @else = null)
            => ExpressionFactory.IfNullElse(expression, then, @else);
    }
}