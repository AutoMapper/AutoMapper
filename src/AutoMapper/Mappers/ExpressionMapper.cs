using System.Collections;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using QueryableExtensions.Impl;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Execution;
    using XpressionMapper.Extensions;

    public class ExpressionMapper : IObjectMapper
    {
        private static TDestination Map<TSource, TDestination>(TSource expression, ResolutionContext context)
            where TSource : LambdaExpression
            where TDestination : LambdaExpression
        {
            return context.Mapper.MapExpression<TDestination>(expression);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ExpressionMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            return typeof(LambdaExpression).IsAssignableFrom(context.SourceType)
                   && context.SourceType != typeof(LambdaExpression)
                   && typeof(LambdaExpression).IsAssignableFrom(context.DestinationType)
                   && context.DestinationType != typeof(LambdaExpression);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, contextExpression);
        }

        internal class MappingVisitor : ExpressionVisitor
        {
            private IList<Type> _destSubTypes = new Type[0];

            private readonly IConfigurationProvider _configurationProvider;
            private readonly TypeMap _typeMap;
            private readonly Expression _oldParam;
            private readonly Expression _newParam;
            private readonly MappingVisitor _parentMappingVisitor;

            public MappingVisitor(IConfigurationProvider configurationProvider, IList<Type> destSubTypes)
                : this(configurationProvider, null, Expression.Parameter(typeof(Nullable)), Expression.Parameter(typeof(Nullable)), null, destSubTypes)
            {
            }

            internal MappingVisitor(IConfigurationProvider configurationProvider, TypeMap typeMap, Expression oldParam, Expression newParam, MappingVisitor parentMappingVisitor = null, IList<Type> destSubTypes = null)
            {
                _configurationProvider = configurationProvider;
                _typeMap = typeMap;
                _oldParam = oldParam;
                _newParam = newParam;
                _parentMappingVisitor = parentMappingVisitor;
                if (destSubTypes != null)
                    _destSubTypes = destSubTypes;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (ReferenceEquals(node, _oldParam))
                    return _newParam;
                return node;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (ReferenceEquals(node, _oldParam))
                    return _newParam;
                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return base.VisitMethodCall(GetConvertedMethodCall(node));
            }

            protected override Expression VisitExtension(Expression node)
            {
                if ((int)node.NodeType == 10000)
                    return node;
                return base.VisitExtension(node);
            }

            private MethodCallExpression GetConvertedMethodCall(MethodCallExpression node)
            {
                if (!node.Method.IsGenericMethod)
                    return node;
                var convertedArguments = Visit(node.Arguments);
                var convertedMethodArgumentTypes = node.Method.GetGenericArguments().Select(t => GetConvertingTypeIfExists(node.Arguments, t, convertedArguments)).ToArray();
                var convertedMethodCall = node.Method.GetGenericMethodDefinition().MakeGenericMethod(convertedMethodArgumentTypes);
                return Expression.Call(convertedMethodCall, convertedArguments);
            }

            private static Type GetConvertingTypeIfExists(IList<Expression> args, Type t, IList<Expression> arguments)
            {
                var matchingArgument = args.Where(a => !a.Type.IsGenericType()).FirstOrDefault(a => a.Type == t);
                if (matchingArgument != null)
                {
                    var index = args.IndexOf(matchingArgument);
                    if (index < 0)
                        return t;
                    return arguments[index].Type;
                }

                var matchingEnumerableArgument = args.Where(a => a.Type.IsGenericType()).FirstOrDefault(a => a.Type.GetTypeInfo().GenericTypeArguments[0] == t);
                var index2 = args.IndexOf(matchingEnumerableArgument);
                if (index2 < 0)
                    return t;
                return arguments[index2].Type.GetTypeInfo().GenericTypeArguments[0];
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                var newLeft = base.Visit(node.Left);
                var newRight = base.Visit(node.Right);

                if (newLeft.Type != newRight.Type && newRight.Type == typeof(string))
                    newLeft = Expression.Call(newLeft, typeof(object).GetDeclaredMethod("ToString"));
                if (newRight.Type != newLeft.Type && newLeft.Type == typeof(string))
                    newRight = Expression.Call(newRight, typeof(object).GetDeclaredMethod("ToString"));
                CheckNullableToNonNullableChanges(node.Left, node.Right, ref newLeft, ref newRight);
                CheckNullableToNonNullableChanges(node.Right, node.Left, ref newRight, ref newLeft);
                return Expression.MakeBinary(node.NodeType, newLeft, newRight);
            }

            private static void CheckNullableToNonNullableChanges(Expression left, Expression right, ref Expression newLeft, ref Expression newRight)
            {
                if (GoingFromNonNullableToNullable(left, newLeft))
                    if (BothAreNonNullable(right, newRight))
                        UpdateToNullableExpression(right, out newRight);
                    else if (BothAreNullable(right, newRight))
                        UpdateToNonNullableExpression(right, out newRight);

                if (GoingFromNonNullableToNullable(newLeft, left))
                    if (BothAreNonNullable(right, newRight))
                        UpdateToNullableExpression(right, out newRight);
                    else if (BothAreNullable(right, newRight))
                        UpdateToNonNullableExpression(right, out newRight);
            }

            private static void UpdateToNullableExpression(Expression right, out Expression newRight)
            {
                if (right is ConstantExpression)
                    newRight = Expression.Constant((right as ConstantExpression).Value,
                        typeof(Nullable<>).MakeGenericType(right.Type));
                else
                    throw new AutoMapperMappingException(
                        "Mapping a BinaryExpression where one side is nullable and the other isn't");
            }

            private static void UpdateToNonNullableExpression(Expression right, out Expression newRight)
            {
                if (right is ConstantExpression)
                {
                    var t = right.Type.IsNullableType()
                        ? right.Type.GetGenericArguments()[0]
                        : right.Type;
                    newRight = Expression.Constant(((ConstantExpression)right).Value, t);
                }
                else if (right is UnaryExpression)
                    newRight = (right as UnaryExpression).Operand;
                else
                    throw new AutoMapperMappingException(
                        "Mapping a BinaryExpression where one side is nullable and the other isn't");
            }

            private static bool GoingFromNonNullableToNullable(Expression node, Expression newLeft)
            {
                return !node.Type.IsNullableType() && newLeft.Type.IsNullableType();
            }

            private static bool BothAreNullable(Expression node, Expression newLeft)
            {
                return node.Type.IsNullableType() && newLeft.Type.IsNullableType();
            }

            private static bool BothAreNonNullable(Expression node, Expression newLeft)
            {
                return !node.Type.IsNullableType() && !newLeft.Type.IsNullableType();
            }

            protected override Expression VisitLambda<T>(Expression<T> expression)
            {
                if (expression.Parameters.Any(b => b.Type == _oldParam.Type))
                    return VisitLambdaExpression(expression);
                return VisitAllParametersExpression(expression);
            }

            private Expression VisitLambdaExpression<T>(Expression<T> expression)
            {
                var convertedBody = base.Visit(expression.Body);
                var convertedArguments = expression.Parameters.Select(e => base.Visit(e) as ParameterExpression).ToList();
                return Expression.Lambda(convertedBody, convertedArguments);
            }

            private Expression VisitAllParametersExpression<T>(Expression<T> expression)
            {
                var visitors = new List<ExpressionVisitor>();
                for (var i = 0; i < expression.Parameters.Count; i++)
                {
                    var sourceParamType = expression.Parameters[i].Type;
                    foreach (var destParamType in _destSubTypes.Where(dt => dt != sourceParamType))
                    {
                        var a = destParamType.IsGenericType() ? destParamType.GetTypeInfo().GenericTypeArguments[0] : destParamType;
                        var typeMap = _configurationProvider.FindTypeMapFor(a, sourceParamType);

                        if (typeMap == null)
                            continue;

                        var oldParam = expression.Parameters[i];
                        var newParam = Expression.Parameter(a, oldParam.Name);
                        visitors.Add(new MappingVisitor(_configurationProvider, typeMap, oldParam, newParam, this));
                    }
                }
                return visitors.Aggregate(expression as Expression, (e, v) => v.Visit(e));
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node == _oldParam)
                    return _newParam;
                var propertyMap = PropertyMap(node);

                if (propertyMap == null)
                {
                    if (node.Expression is MemberExpression)
                        return GetConvertedSubMemberCall(node);
                    return node;
                }

                var constantVisitor = new IsConstantExpressionVisitor();
                constantVisitor.Visit(node);
                if (constantVisitor.IsConstant)
                    return node;

                SetSorceSubTypes(propertyMap);

                var replacedExpression = Visit(node.Expression);
                if (replacedExpression == node.Expression)
                    replacedExpression = _parentMappingVisitor.Visit(node.Expression);

                if (propertyMap.CustomExpression != null)
                    return propertyMap.CustomExpression.ReplaceParameters(replacedExpression);

                Func<Expression, MemberInfo, Expression> getExpression = Expression.MakeMemberAccess;

                return propertyMap.SourceMembers
                    .Aggregate(replacedExpression, getExpression);
            }

            private class IsConstantExpressionVisitor : ExpressionVisitor
            {
                public bool IsConstant { get; private set; }

                protected override Expression VisitConstant(ConstantExpression node)
                {
                    IsConstant = true;

                    return base.VisitConstant(node);
                }
            }

            private Expression GetConvertedSubMemberCall(MemberExpression node)
            {
                var baseExpression = Visit(node.Expression);
                var propertyMap = FindPropertyMapOfExpression(node.Expression as MemberExpression);
                if (propertyMap == null)
                    return node;
                var sourceType = propertyMap.SourceMember.GetMemberType();
                var destType = propertyMap.DestinationPropertyType;
                if (sourceType == destType)
                    return Expression.MakeMemberAccess(baseExpression, node.Member);
                var typeMap = _configurationProvider.FindTypeMapFor(sourceType, destType);
                var subVisitor = new MappingVisitor(_configurationProvider, typeMap, node.Expression, baseExpression, this);
                var newExpression = subVisitor.Visit(node);
                _destSubTypes = _destSubTypes.Concat(subVisitor._destSubTypes).ToArray();
                return newExpression;
            }

            private PropertyMap FindPropertyMapOfExpression(MemberExpression expression)
            {
                var propertyMap = PropertyMap(expression);
                if (propertyMap == null && expression.Expression is MemberExpression)
                    return FindPropertyMapOfExpression(expression.Expression as MemberExpression);
                return propertyMap;
            }

            private PropertyMap PropertyMap(MemberExpression node)
            {
                if (_typeMap == null)
                    return null;

                if (node.Member.IsStatic())
                    return null;

                var memberAccessor = node.Member;

                // in case of a propertypath, the MemberAcessors type and the SourceType may be different
                if (!memberAccessor.DeclaringType.IsAssignableFrom(_typeMap.DestinationType))
                    return null;

                var propertyMap = _typeMap.GetExistingPropertyMapFor(memberAccessor);
                return propertyMap;
            }

            private void SetSorceSubTypes(PropertyMap propertyMap)
            {
                if (propertyMap.SourceMember is PropertyInfo)
                    _destSubTypes = (propertyMap.SourceMember as PropertyInfo).PropertyType.GetTypeInfo().GenericTypeArguments.Concat(new[] { (propertyMap.SourceMember as PropertyInfo).PropertyType }).ToList();
                else if (propertyMap.SourceMember is FieldInfo)
                    _destSubTypes = (propertyMap.SourceMember as FieldInfo).FieldType.GetTypeInfo().GenericTypeArguments;
            }
        }
    }
}
