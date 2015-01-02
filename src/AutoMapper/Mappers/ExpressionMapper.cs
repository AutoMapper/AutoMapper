using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internal;

    public class ExpressionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var sourceDelegateType = context.SourceType.GetGenericArguments()[0];
            var destDelegateType = context.DestinationType.GetGenericArguments()[0];
            var expression = (LambdaExpression) context.SourceValue;

            if (sourceDelegateType.GetGenericTypeDefinition() != destDelegateType.GetGenericTypeDefinition())
                throw new AutoMapperMappingException("Source and destination expressions must be of the same type.");

            var mappingVisitor = new MappingVisitor(destDelegateType.GetGenericArguments());
            return mappingVisitor.Visit(expression);
        }

        public bool IsMatch(ResolutionContext context)
        {
            return typeof (LambdaExpression).IsAssignableFrom(context.SourceType)
                   && context.SourceType != typeof (LambdaExpression)
                   && typeof (LambdaExpression).IsAssignableFrom(context.DestinationType)
                   && context.DestinationType != typeof (LambdaExpression);
        }

        private class MappingVisitor : ExpressionVisitor
        {
            private IList<Type> _destSubTypes;

            private readonly TypeMap _typeMap;
            private readonly Expression _oldParam;
            private readonly Expression _newParam;

            public MappingVisitor(IList<Type> destSubTypes)
                : this(null, Expression.Parameter(typeof(Nullable)), Expression.Parameter(typeof(Nullable)) )
            {
                _destSubTypes = destSubTypes;
            }

            private MappingVisitor(TypeMap typeMap, Expression oldParam, Expression newParam)
            {
                _typeMap = typeMap;
                _oldParam = oldParam;
                _newParam = newParam;
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

            private MethodCallExpression GetConvertedMethodCall(MethodCallExpression node)
            {
                var convertedArguments = Visit(node.Arguments);
                var convertedMethodArgumentTypes = node.Method.GetGenericArguments().Select(t => GetConvertingTypeIfExists(node.Arguments, t, convertedArguments)).ToArray();
                var convertedMethodCall = node.Method.GetGenericMethodDefinition().MakeGenericMethod(convertedMethodArgumentTypes);
                return Expression.Call(convertedMethodCall, convertedArguments);
            }

            private static Type GetConvertingTypeIfExists(IList<Expression> args, Type t, IList<Expression> arguments)
            {
                var matchingArgument = args.Where(a => !a.Type.IsGenericType).FirstOrDefault(a => a.Type == t);
                if (matchingArgument != null)
                {
                    var index = args.IndexOf(matchingArgument);
                    if (index < 0)
                        return t;
                    return arguments[index].Type;
                }

                var matchingEnumerableArgument = args.Where(a => a.Type.IsGenericType).FirstOrDefault(a => a.Type.GetGenericArguments()[0] == t);
                var index2 = args.IndexOf(matchingEnumerableArgument);
                if (index2 < 0) 
                    return t;
                return arguments[index2].Type.GetGenericArguments()[0];
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
                    if (_destSubTypes.Count <= i)
                        continue;
                    var destParamType = _destSubTypes[i];
                    if (sourceParamType == destParamType)
                        continue;

                    var typeMap = Mapper.FindTypeMapFor(destParamType, sourceParamType);

                    if (typeMap == null)
                        throw new AutoMapperMappingException(
                            string.Format(
                                "Could not find type map from destination type {0} to source type {1}. Use CreateMap to create a map from the source to destination types.",
                                destParamType, sourceParamType));

                    var oldParam = expression.Parameters[i];
                    var newParam = Expression.Parameter(typeMap.SourceType, oldParam.Name);
                    visitors.Add(new MappingVisitor(typeMap, oldParam, newParam));
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

                if (propertyMap.CustomExpression != null)
                    return ConvertCustomExpression(replacedExpression, propertyMap);
                
                return propertyMap.GetSourceValueResolvers()
                    .OfType<IMemberGetter>()
                    .Aggregate(replacedExpression,
                        (current, memberGetter) => Expression.MakeMemberAccess(current, memberGetter.MemberInfo));
            }

            private Expression GetConvertedSubMemberCall(MemberExpression node)
            {
                var baseExpression = Visit(node.Expression);
                var propertyMap = FindPropertyMapOfExpression(node.Expression as MemberExpression);

                var sourceType = propertyMap.SourceMember.GetMemberType();
                var destType = propertyMap.DestinationPropertyType;
                if (sourceType == destType)
                    return Expression.MakeMemberAccess(baseExpression, node.Member);
                var typeMap = Mapper.FindTypeMapFor(sourceType, destType);
                var memberExpression = new MappingVisitor(typeMap, node.Expression, baseExpression).Visit(node) as MemberExpression;

                return Expression.MakeMemberAccess(baseExpression,memberExpression.Member);
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
                var memberAccessor = node.Member.ToMemberAccessor();
                var propertyMap = _typeMap.GetExistingPropertyMapFor(memberAccessor);
                return propertyMap;
            }

            private void SetSorceSubTypes(PropertyMap propertyMap)
            {
                if (propertyMap.SourceMember is PropertyInfo)
                    _destSubTypes = (propertyMap.SourceMember as PropertyInfo).PropertyType.GetGenericArguments();
                else if (propertyMap.SourceMember is FieldInfo)
                    _destSubTypes = (propertyMap.SourceMember as FieldInfo).FieldType.GetGenericArguments();
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
