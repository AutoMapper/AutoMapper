﻿using System.Collections;
using Expression = System.Linq.Expressions.Expression;

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
    using static Expression;

    public class ExpressionMapper : IObjectMapper
    {
        public static TDestination Map<TSource, TDestination>(TSource expression, ResolutionContext context)
            where TSource : LambdaExpression
            where TDestination : LambdaExpression
        {
            var sourceDelegateType = typeof(TSource).GetTypeInfo().GenericTypeArguments[0];
            var destDelegateType = typeof(TDestination).GetTypeInfo().GenericTypeArguments[0];

            if (sourceDelegateType.GetGenericTypeDefinition() != destDelegateType.GetGenericTypeDefinition())
                throw new AutoMapperMappingException("Source and destination expressions must be of the same type.", null, new TypePair(typeof(TSource), typeof(TDestination)));

            var dictionary = expression.Parameters.Select((p, i) =>
            {
                var dest = destDelegateType.GetTypeInfo().GenericTypeArguments[i];
                if (dest.IsGenericType())
                    dest = dest.GetTypeInfo().GenericTypeArguments[i];
                var src = sourceDelegateType.GetTypeInfo().GenericTypeArguments[i];
                if (src.IsGenericType())
                    src = src.GetTypeInfo().GenericTypeArguments[i];

                var tm = context.ConfigurationProvider.ResolveTypeMap(dest, src);
                return new Translation(tm, p,
                    Parameter(destDelegateType.GetTypeInfo().GenericTypeArguments[i], expression.Parameters[i].Name));
            }).ToArray();

            var parentMasterVisitor = new MappingVisitor(context.ConfigurationProvider,
                destDelegateType.GetTypeInfo().GenericTypeArguments);
            var typeMapVisitor = new MappingVisitor(context.ConfigurationProvider,
                destDelegateType.GetTypeInfo().GenericTypeArguments, parentMasterVisitor, dictionary);

            // Map expression body and variable seperately
            var parameters = expression.Parameters.Select(typeMapVisitor.Visit).OfType<ParameterExpression>();
            var body = typeMapVisitor.Visit(expression.Body);
            return (TDestination)Lambda(ExpressionExtensions.ToType(body, destDelegateType.GetTypeInfo().GenericTypeArguments.Last()), parameters);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ExpressionMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return typeof(LambdaExpression).IsAssignableFrom(context.SourceType)
                   && context.SourceType != typeof(LambdaExpression)
                   && typeof(LambdaExpression).IsAssignableFrom(context.DestinationType)
                   && context.DestinationType != typeof(LambdaExpression);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type), sourceExpression, contextExpression);
        }

        public class Translation
        {
            public TypeMap TypeMap { get; }
            public IList<FromTo> FromTos { get; } = new List<FromTo>();

            public Translation(TypeMap typeMap, Expression from, Expression to)
                : this(typeMap)
            {
                TypeMap = typeMap;
                FromTos.Add(new FromTo(@from, to));
            }

            public Translation(TypeMap typeMap)
            {
                TypeMap = typeMap;
            }
        }

        public class FromTo
        {
            public Expression From { get; }
            public Expression To { get; }

            public FromTo(Expression from, Expression to)
            {
                From = from;
                To = to;
            }
        }

        internal class MappingVisitor : ExpressionVisitor
        {
            private IList<Type> _destSubTypes = new Type[0];

            private readonly IConfigurationProvider _configurationProvider;
            private readonly IList<Translation> _translations = new List<Translation>();
            private readonly MappingVisitor _parentMappingVisitor;

            internal MappingVisitor(IConfigurationProvider configurationProvider, IList<Type> destSubTypes = null, MappingVisitor parentMappingVisitor = null, params Translation[] translations)
            {
                _configurationProvider = configurationProvider;
                foreach (var translation in translations)
                    _translations.Add(translation);
                _parentMappingVisitor = parentMappingVisitor;
                if (destSubTypes != null)
                    _destSubTypes = destSubTypes;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                foreach (var fromTo in _translations.SelectMany(t => t.FromTos))
                    if (ReferenceEquals(node, fromTo.From))
                        return fromTo.To;
                return node;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                foreach (var translations in _translations)
                    foreach (var translation in translations.FromTos)
                        if (ReferenceEquals(node, translation.From))
                            return translation.To;
                foreach (var translation in _translations)
                    foreach (var fromTo in translation.FromTos)
                        if (node.Type == fromTo.From.Type)
                        {
                            var to = Parameter(fromTo.To.Type, node.Name);
                            translation.FromTos.Add(new FromTo(node, to));
                            return to;
                        }
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
                return Call(convertedMethodCall, convertedArguments);
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
                    newLeft = Call(newLeft, typeof(object).GetDeclaredMethod("ToString"));
                if (newRight.Type != newLeft.Type && newLeft.Type == typeof(string))
                    newRight = Call(newRight, typeof(object).GetDeclaredMethod("ToString"));
                CheckNullableToNonNullableChanges(node.Left, node.Right, ref newLeft, ref newRight);
                CheckNullableToNonNullableChanges(node.Right, node.Left, ref newRight, ref newLeft);
                return MakeBinary(node.NodeType, newLeft, newRight);
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
                    newRight = Constant((right as ConstantExpression).Value,
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
                    newRight = Constant(((ConstantExpression)right).Value, t);
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
                foreach (var translation in _translations.SelectMany(t => t.FromTos))
                    if (expression.Parameters.Any(b => b.Type == translation.From.Type))
                        return VisitLambdaExpression(expression);
                return VisitAllParametersExpression(expression);
            }

            private Expression VisitLambdaExpression<T>(Expression<T> expression)
            {
                var convertedBody = base.Visit(expression.Body);
                var convertedArguments = expression.Parameters.Select(e => base.Visit(e) as ParameterExpression).ToList();
                return Lambda(convertedBody, convertedArguments);
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
                        var newParam = Parameter(a, oldParam.Name);
                        visitors.Add(new MappingVisitor(_configurationProvider, null, this,
                            new Translation(typeMap) { FromTos = { new FromTo(oldParam, newParam) } }));
                    }
                }
                return visitors.Aggregate(expression as Expression, (e, v) => v.Visit(e));
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                foreach (var fromTos in _translations.SelectMany(t => t.FromTos))
                    if (node == fromTos.From)
                        return fromTos.To;
                var propertyMap = PropertyMap(node);

                if (propertyMap == null)
                {
                    if (node.Expression is MemberExpression)
                        return GetConvertedSubMemberCall(node);
                    return _parentMappingVisitor != null ? _parentMappingVisitor.Visit(node) : node;
                }

                var constantVisitor = new IsConstantExpressionVisitor();
                constantVisitor.Visit(node);
                if (constantVisitor.IsConstant)
                    return node;

                SetSorceSubTypes(propertyMap);

                var replacedExpression = Visit(node.Expression);
                if (replacedExpression == node.Expression)
                    return _parentMappingVisitor != null ? _parentMappingVisitor.Visit(node) : node;

                if (propertyMap.CustomExpression != null)
                    return propertyMap.CustomExpression.ReplaceParameters(replacedExpression);

                Func<Expression, MemberInfo, Expression> getExpression = MakeMemberAccess;

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
                var sourceType = propertyMap.SourceType;
                var destType = propertyMap.DestinationPropertyType;
                if (sourceType == destType)
                    return MakeMemberAccess(baseExpression, node.Member);
                var typeMap = _configurationProvider.FindTypeMapFor(sourceType, destType);
                var subVisitor = new MappingVisitor(_configurationProvider, null, this, new Translation(typeMap, node.Expression, baseExpression));
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
                if (node.Member.IsStatic())
                    return null;

                foreach (var typeMap in _translations.Select(t => t.TypeMap))
                    if (node.Member.DeclaringType.IsAssignableFrom(typeMap.DestinationType))
                        return typeMap.GetExistingPropertyMapFor(node.Member);

                return null;
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
