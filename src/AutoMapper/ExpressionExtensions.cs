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
            members
                .Aggregate(
                        obj,
                        (inner, getter) => getter is MethodInfo method ?
                            (getter.IsStatic() ? Call(null, method, inner) : (Expression)Call(inner, method)) :
                            MakeMemberAccess(getter.IsStatic() ? null : inner, getter));

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
            return GetMembersCore().Reverse();
            IEnumerable<MemberExpression> GetMembersCore()
            {
                while (expression != null)
                {
                    yield return expression;
                    expression = expression.Expression as MemberExpression;
                }
            }
        }

        public static void EnsureMemberPath(this LambdaExpression exp, string name)
        {
            if(!exp.IsMemberPath())
            {
                throw new ArgumentOutOfRangeException(name, "Only member accesses are allowed. "+exp);
            }
        }

        public static bool IsMemberPath(this LambdaExpression exp) => exp.Body.GetMembers().FirstOrDefault()?.Expression == exp.Parameters.First();

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

        public static LambdaExpression[] WithoutCastToObject<T>(this Expression<Func<T, object>>[] expressions)
            => Array.ConvertAll(
                expressions,
                e =>
                {
                    var bodyIsCastToObject = (e.Body.NodeType == ExpressionType.Convert || e.Body.NodeType == ExpressionType.ConvertChecked) && e.Body.Type == typeof(object);
                    return bodyIsCastToObject ? Lambda(((UnaryExpression)e.Body).Operand, e.Parameters) : e;
                });
    }
}