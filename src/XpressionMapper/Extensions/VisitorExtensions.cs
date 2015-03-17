using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XpressionMapper.Extensions
{
    internal static class VisitorExtensions
    {
        /// <summary>
        /// Returns true if the expression is a direct or descendant member expression of the parameter.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsMemberExpression(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                return IsMemberOrParameterExpression(memberExpression.Expression);
            }

            return false;
        }

        private static bool IsMemberOrParameterExpression(Expression expression)
        {
            //the node represents parameter of the expression
            if (expression.NodeType == ExpressionType.Parameter)
                return true;

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                return IsMemberOrParameterExpression(memberExpression.Expression);
            }

            return false;
        }

        /// <summary>
        /// Returns the fully qualified name of the member starting with the immediate child member of the parameter
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyFullName(this Expression expression)
        {
            const string PERIOD = ".";

            //the node represents parameter of the expression
            if (expression.NodeType == ExpressionType.Parameter)
                return string.Empty;

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberExpression = (MemberExpression)expression;
                string parentFullName = memberExpression.Expression.GetPropertyFullName();
                return string.IsNullOrEmpty(parentFullName)
                    ? memberExpression.Member.Name
                    : string.Concat(memberExpression.Expression.GetPropertyFullName(), PERIOD, memberExpression.Member.Name);
            }

            throw new InvalidOperationException(Properties.Resources.invalidExpErr);
        }

        /// <summary>
        /// Returns the Systen.Type for the LINQ parameter.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Type GetParameterType(this Expression expression)
        {
            if (expression == null)
                return null;

            //the node represents parameter of the expression
            if (expression.NodeType == ExpressionType.Parameter)
                return expression.Type;

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                return GetParameterType(memberExpression.Expression);
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var methodExpression = expression as MethodCallExpression;
                var memberExpression = methodExpression.Object as MemberExpression;//Method is an instance method

                if (memberExpression == null && methodExpression.Arguments.Count > 0)
                    memberExpression = methodExpression.Arguments[0] as MemberExpression;//Method is an extension method based on the type of methodExpression.Arguments[0] and methodExpression.Arguments[0] is a member expression.

                if (memberExpression == null && methodExpression.Arguments.Count > 0)
                    return GetParameterType(methodExpression.Arguments[0]);//Method is an extension method based on the type of methodExpression.Arguments[0] but methodExpression.Arguments[0] is not a member expression.
                else
                    return memberExpression == null ? null : GetParameterType(memberExpression.Expression);
            }

            return null;
        }

        /// <summary>
        /// Builds and new member expression for the given parameter given its Type and fullname
        /// </summary>
        /// <param name="newParameter"></param>
        /// <param name="type"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static MemberExpression BuildExpression(this ParameterExpression newParameter, Type type, string fullName)
        {
            var parts = fullName.Split('.');

            Expression parent = newParameter;
            foreach (var part in parts)
            {
                PropertyInfo info = type.GetProperty(part);
                parent = Expression.Property(parent, info);
                type = info.PropertyType;
            }

            return (MemberExpression)parent;
        }

        /// <summary>
        /// For the given a Lambda Expression, returns the fully qualified name of the member starting with the immediate child member of the parameter
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetMemberFullName(this LambdaExpression expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expr.Body as UnaryExpression;
                    me = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            return me.GetPropertyFullName();
        }
    }
}
