using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.XpressionMapper.Structures;

namespace AutoMapper.XpressionMapper.Extensions
{
    using AutoMapper.Internal;
    using Configuration;

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
            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return true;
                case ExpressionType.MemberAccess:
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
            const string period = ".";

            //the node represents parameter of the expression
            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return string.Empty;
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    var parentFullName = memberExpression.Expression.GetPropertyFullName();
                    return string.IsNullOrEmpty(parentFullName)
                        ? memberExpression.Member.Name
                        : string.Concat(memberExpression.Expression.GetPropertyFullName(), period, memberExpression.Member.Name);
            }

            throw new InvalidOperationException(Resource.invalidExpErr);
        }

        private static MemberExpression GetMemberExpression(LambdaExpression expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expr.Body as UnaryExpression;
                    me = ue?.Operand as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    if (me == null)
                    {
                        if (expr.Body is BinaryExpression binaryExpression)
                        {
                            if (binaryExpression.Left is MemberExpression left)
                                return left;
                            if (binaryExpression.Right is MemberExpression right)
                                return right;
                        }
                    }
                    break;
            }

            return me;
        }

        /// <summary>
        /// Returns the ParameterExpression for the LINQ parameter.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static ParameterExpression GetParameterExpression(this Expression expression)
        {
            if (expression == null)
                return null;

            //the node represents parameter of the expression
            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return (ParameterExpression)expression;
                case ExpressionType.Quote:
                    return GetParameterExpression(GetMemberExpression((LambdaExpression)((UnaryExpression)expression).Operand));
                case ExpressionType.Lambda:
                    return GetParameterExpression(GetMemberExpression((LambdaExpression)expression));
                case ExpressionType.ConvertChecked:
                case ExpressionType.Convert:
                    var ue = expression as UnaryExpression;
                    return GetParameterExpression(ue?.Operand);
                case ExpressionType.MemberAccess:
                    return GetParameterExpression(((MemberExpression)expression).Expression);
                case ExpressionType.Call:
                    var methodExpression = expression as MethodCallExpression;
                    var memberExpression = methodExpression?.Object as MemberExpression;//Method is an instance method

                    var isExtension = methodExpression != null && methodExpression.Method.IsDefined(typeof(ExtensionAttribute), true);
                    if (isExtension && memberExpression == null && methodExpression.Arguments.Count > 0)
                        memberExpression = methodExpression.Arguments[0] as MemberExpression;//Method is an extension method based on the type of methodExpression.Arguments[0] and methodExpression.Arguments[0] is a member expression.

                    return isExtension && memberExpression == null && methodExpression.Arguments.Count > 0
                        ? GetParameterExpression(methodExpression.Arguments[0])
                        : (memberExpression == null ? null : GetParameterExpression(memberExpression.Expression));
            }

            return null;
        }

        /// <summary>
        /// Adds member expressions to an existing expression.
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static MemberExpression MemberAccesses(this Expression exp, List<PropertyMapInfo> list) =>
            (MemberExpression) list.SelectMany(propertyMapInfo => propertyMapInfo.DestinationPropertyInfos).MemberAccesses(exp);

        /// <summary>
        /// For the given a Lambda Expression, returns the fully qualified name of the member starting with the immediate child member of the parameter
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetMemberFullName(this LambdaExpression expr)
        {
            if (expr.Body.NodeType == ExpressionType.Parameter)
                return string.Empty;

            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    me = (expr.Body as UnaryExpression)?.Operand as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            return me.GetPropertyFullName();
        }

        /// <summary>
        /// Returns the underlying type typeof(T) when the type implements IEnumerable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Type> GetUnderlyingGenericTypes(this Type type) => 
            type == null || !type.GetTypeInfo().IsGenericType
            ? new List<Type>()
            : type.GetGenericArguments().ToList();
    }
}
