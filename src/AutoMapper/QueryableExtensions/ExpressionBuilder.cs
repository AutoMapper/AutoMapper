using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.QueryableExtensions
{
    public interface IExpressionBuilder
    {
        Expression CreateMapExpression(Type sourceType, Type destinationType, IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand);
        Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand);
        LambdaExpression CreateMapExpression(ExpressionRequest request, ConcurrentDictionary<ExpressionRequest, int> typePairCount);
        Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, ConcurrentDictionary<ExpressionRequest, int> typePairCount);
    }

    public class ExpressionBuilder : IExpressionBuilder
    {
        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberGetterExpressionResultConverter(),
            new MemberResolverExpressionResultConverter(),
            new NullSubstitutionExpressionResultConverter()
        };

        private static readonly IExpressionBinder[] Binders =
        {
            new NullableExpressionBinder(),
            new AssignableExpressionBinder(),
            new EnumerableExpressionBinder(),
            new MappedTypeExpressionBinder(),
            new CustomProjectionExpressionBinder(),
            new StringExpressionBinder()
        };

        private readonly ConcurrentDictionary<ExpressionRequest, LambdaExpression> _expressionCache = new ConcurrentDictionary<ExpressionRequest, LambdaExpression>();
        private readonly IConfigurationProvider _configurationProvider;

        public ExpressionBuilder(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public Expression CreateMapExpression(Type sourceType, Type destinationType, IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand)
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var cachedExpression =
                _expressionCache.GetOrAdd(new ExpressionRequest(sourceType, destinationType, membersToExpand),
                    tp => CreateMapExpression(tp, new ConcurrentDictionary<ExpressionRequest, int>()));

            if (!parameters.Any())
                return cachedExpression;

            var visitor = new ConstantExpressionReplacementVisitor(parameters);

            return visitor.Visit(cachedExpression);
        }

        public Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(IDictionary<string, object> parameters = null,
            params MemberInfo[] membersToExpand)
        {
            return (Expression<Func<TSource, TDestination>>) CreateMapExpression(typeof(TSource), typeof(TDestination), parameters, membersToExpand);
        }


        public LambdaExpression CreateMapExpression(ExpressionRequest request, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            var instanceParameter = Expression.Parameter(request.SourceType, "dto");
            var total = CreateMapExpression(request, instanceParameter, typePairCount);
            var delegateType = typeof(Func<,>).MakeGenericType(request.SourceType, request.DestinationType);
            return Expression.Lambda(delegateType, total, instanceParameter);
        }

        public Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            var typeMap = _configurationProvider.ResolveTypeMap(request.SourceType,
                request.DestinationType);

            if (typeMap == null)
            {
                throw QueryMapperHelper.MissingMapException(request.SourceType, request.DestinationType);
            }

            var parameterReplacer = instanceParameter is ParameterExpression ? new ParameterReplacementVisitor(instanceParameter) : null;
            var customProjection = typeMap.CustomProjection;
            if (customProjection != null)
            {
                return parameterReplacer == null ? customProjection.Body : parameterReplacer.Visit(customProjection.Body);
            }

            var bindings = new List<MemberBinding>();
            var visitCount = typePairCount.AddOrUpdate(request, 0, (tp, i) => i + 1);
            if (visitCount >= typeMap.MaxDepth)
            {
                if (_configurationProvider.AllowNullDestinationValues)
                {
                    return null;
                }
            }
            else
            {
                bindings = CreateMemberBindings(request, typeMap, instanceParameter, typePairCount);
            }
            Expression constructorExpression = typeMap.DestinationConstructorExpression(instanceParameter);
            if (parameterReplacer != null)
            {
                constructorExpression = parameterReplacer.Visit(constructorExpression);
            }
            var visitor = new NewFinderVisitor();
            visitor.Visit(constructorExpression);

            var expression = Expression.MemberInit(
                visitor.NewExpression,
                bindings.ToArray()
                );
            return expression;
        }

        private class NewFinderVisitor : ExpressionVisitor
        {
            public NewExpression NewExpression { get; private set; }

            protected override Expression VisitNew(NewExpression node)
            {
                NewExpression = node;
                return base.VisitNew(node);
            }
        }

        private List<MemberBinding> CreateMemberBindings(ExpressionRequest request,
            TypeMap typeMap,
            Expression instanceParameter, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            var bindings = new List<MemberBinding>();

            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var result = ResolveExpression(propertyMap, request.SourceType, instanceParameter);

                if (propertyMap.ExplicitExpansion &&
                    !request.MembersToExpand.Contains(propertyMap.DestinationProperty.MemberInfo))
                    continue;

                var propertyTypeMap = _configurationProvider.ResolveTypeMap(result.Type,
                    propertyMap.DestinationPropertyType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationPropertyType, request.MembersToExpand);

                var binder = Binders.FirstOrDefault(b => b.IsMatch(propertyMap, propertyTypeMap, result));

                if (binder == null)
                {
                    var message =
                        $"Unable to create a map expression from {propertyMap.SourceMember?.DeclaringType?.Name}.{propertyMap.SourceMember?.Name} ({result.Type}) to {propertyMap.DestinationProperty.MemberInfo.DeclaringType?.Name}.{propertyMap.DestinationProperty.Name} ({propertyMap.DestinationPropertyType})";

                    throw new AutoMapperMappingException(message);
                }

                var bindExpression = binder.Build(_configurationProvider, propertyMap, propertyTypeMap, propertyRequest, result, typePairCount);

                if (bindExpression != null)
                {
                    bindings.Add(bindExpression);
                }
            }
            return bindings;
        }

        private static ExpressionResolutionResult ResolveExpression(PropertyMap propertyMap, Type currentType,
            Expression instanceParameter)
        {
            var result = new ExpressionResolutionResult(instanceParameter, currentType);
            foreach (var resolver in propertyMap.GetSourceValueResolvers())
            {
                var matchingExpressionConverter =
                    ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, resolver));
                if (matchingExpressionConverter == null)
                    throw new Exception("Can't resolve this to Queryable Expression");
                result = matchingExpressionConverter.GetExpressionResolutionResult(result, propertyMap, resolver);
            }
            return result;
        }

        private class ConstantExpressionReplacementVisitor : ExpressionVisitor
        {
            private readonly IDictionary<string, object> _paramValues;

            public ConstantExpressionReplacementVisitor(
                IDictionary<string, object> paramValues)
            {
                _paramValues = paramValues;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!node.Member.DeclaringType.Name.Contains("<>"))
                    return base.VisitMember(node);

                if (!_paramValues.ContainsKey(node.Member.Name))
                    return base.VisitMember(node);

                return Expression.Convert(
                    Expression.Constant(_paramValues[node.Member.Name]),
                    node.Member.GetMemberType());
            }
        }
    }
}