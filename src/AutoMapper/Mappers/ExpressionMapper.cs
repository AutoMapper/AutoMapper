namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using QueryableExtensions.Impl;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internal;

    public class ExpressionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            var sourceDelegateType = context.SourceType.GetTypeInfo().GenericTypeArguments[0];
            var destDelegateType = context.DestinationType.GetTypeInfo().GenericTypeArguments[0];
            var expression = (LambdaExpression) context.SourceValue;

            if (sourceDelegateType.GetGenericTypeDefinition() != destDelegateType.GetGenericTypeDefinition())
                throw new AutoMapperMappingException("Source and destination expressions must be of the same type.");

            var destArgType = destDelegateType.GetTypeInfo().GenericTypeArguments[0];
            if (destArgType.IsGenericType())
                destArgType = destArgType.GetTypeInfo().GenericTypeArguments[0];
            var sourceArgType = sourceDelegateType.GetTypeInfo().GenericTypeArguments[0];
            if (sourceArgType.IsGenericType())
                sourceArgType = sourceArgType.GetTypeInfo().GenericTypeArguments[0];

            var typeMap = context.ConfigurationProvider.ResolveTypeMap(destArgType, sourceArgType);

            var parentMasterVisitor = new MappingVisitor(context.ConfigurationProvider, destDelegateType.GetTypeInfo().GenericTypeArguments);
            var typeMapVisitor = new MappingVisitor(context.ConfigurationProvider, typeMap, expression.Parameters[0], Expression.Parameter(destDelegateType.GetTypeInfo().GenericTypeArguments[0], expression.Parameters[0].Name), parentMasterVisitor, destDelegateType.GetTypeInfo().GenericTypeArguments);
            
            // Map expression body and variable seperately
            var parameters = expression.Parameters.Select(typeMapVisitor.Visit).OfType<ParameterExpression>();
            var body = typeMapVisitor.Visit(expression.Body);
            return Expression.Lambda(body, parameters);
        }

        public bool IsMatch(TypePair context)
        {
            return typeof (LambdaExpression).IsAssignableFrom(context.SourceType)
                   && context.SourceType != typeof (LambdaExpression)
                   && typeof (LambdaExpression).IsAssignableFrom(context.DestinationType)
                   && context.DestinationType != typeof (LambdaExpression);
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
                if(destSubTypes != null)
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
                        typeof (Nullable<>).MakeGenericType(right.Type));
                else
                    throw new AutoMapperMappingException(
                        "Mapping a BinaryExpression where one side is nullable and the other isn't");
            }

            private static void UpdateToNonNullableExpression(Expression right, out Expression newRight)
            {
                if (right is ConstantExpression)
                    newRight = Expression.Constant((right as ConstantExpression).Value,
                        typeof(Nullable<>).MakeGenericType(right.Type));
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
                        var a = destParamType.IsGenericType() ? destParamType.GetTypeInfo().GenericTypeArguments[0]: destParamType;
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
                SetSorceSubTypes(propertyMap);

                var replacedExpression = Visit(node.Expression);
                if (replacedExpression == node.Expression)
                    replacedExpression = _parentMappingVisitor.Visit(node.Expression);

                if (propertyMap.CustomExpression != null)
                    return ConvertCustomExpression(replacedExpression, propertyMap);

                Func<Expression,IMemberGetter,Expression> getExpression = (current, memberGetter) => Expression.MakeMemberAccess(current, memberGetter.MemberInfo);

                //if (propertyMap.SourceMember.ToMemberGetter().MemberType.IsNullableType())
                //{
                //    var expression = getExpression;
                //    getExpression = (current, memberGetter) => Expression.Call(expression.Invoke(current,memberGetter), "GetValueOrDefault", new Type[0], new Expression[0]);
                //}
                //else if (propertyMap.DestinationPropertyType.IsNullableType())
                //{

                //}

                return propertyMap.GetSourceValueResolvers()
                    .OfType<IMemberGetter>()
                    .Aggregate(replacedExpression, getExpression);
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
                if (node.Member.IsStatic())
                    return null;

                var memberAccessor = node.Member.ToMemberAccessor();
                var propertyMap = _typeMap.GetExistingPropertyMapFor(memberAccessor);
                return propertyMap;
            }

            private void SetSorceSubTypes(PropertyMap propertyMap)
            {
                if (propertyMap.SourceMember is PropertyInfo)
                    _destSubTypes = (propertyMap.SourceMember as PropertyInfo).PropertyType.GetTypeInfo().GenericTypeArguments.Concat(new []{ (propertyMap.SourceMember as PropertyInfo).PropertyType }).ToList();
                else if (propertyMap.SourceMember is FieldInfo)
                    _destSubTypes = (propertyMap.SourceMember as FieldInfo).FieldType.GetTypeInfo().GenericTypeArguments;
            }

            private Expression ConvertCustomExpression(Expression node, PropertyMap propertyMap)
            {
                var replaced = new ParameterReplacementVisitor(node);
                var newBody = replaced.Visit(propertyMap.CustomExpression.Body);
                return newBody;
            }
        }
    }
}
