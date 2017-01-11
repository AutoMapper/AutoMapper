using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.QueryableExtensions.Impl;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.QueryableExtensions
{
    using System.Runtime.CompilerServices;
    using Configuration;
    using Execution;

    public interface IExpressionBuilder
    {
        Expression CreateMapExpression(Type sourceType, Type destinationType, IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand);
        Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand);
        LambdaExpression CreateMapExpression(ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount);
        Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount);
    }

    public class ExpressionBuilder : IExpressionBuilder
    {
        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberResolverExpressionResultConverter(),
            new MemberGetterExpressionResultConverter(),
        };

        private static readonly IExpressionBinder[] Binders =
        {
            new CustomProjectionExpressionBinder(),
            new NullableExpressionBinder(),
            new AssignableExpressionBinder(),
            new EnumerableExpressionBinder(),
            new MappedTypeExpressionBinder(),
            new StringExpressionBinder()
        };

        private readonly LockingConcurrentDictionary<ExpressionRequest, LambdaExpression> _expressionCache;
        private readonly IConfigurationProvider _configurationProvider;

        public ExpressionBuilder(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
            _expressionCache = new LockingConcurrentDictionary<ExpressionRequest, LambdaExpression>(CreateMapExpression);
        }

        public Expression CreateMapExpression(Type sourceType, Type destinationType, IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand)
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var cachedExpression = _expressionCache.GetOrAdd(new ExpressionRequest(sourceType, destinationType, membersToExpand, null));

            Expression x = cachedExpression;
            if (parameters.Any())
            {
                var visitor = new ConstantExpressionReplacementVisitor(parameters);
                x = visitor.Visit(cachedExpression);
            }

            // perform null-propagation if this feature is enabled.
            if (_configurationProvider.EnableNullPropagationForQueryMapping)
            {
                var nullVisitor = new NullsafeQueryRewriter();
                return nullVisitor.Visit(x);
            }
            return x;
        }

        public Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(IDictionary<string, object> parameters = null,
            params MemberInfo[] membersToExpand)
        {
            return (Expression<Func<TSource, TDestination>>)CreateMapExpression(typeof(TSource), typeof(TDestination), parameters, membersToExpand);
        }

        public LambdaExpression CreateMapExpression(ExpressionRequest request)
        {
            return CreateMapExpression(request, new Dictionary<ExpressionRequest, int>());
        }

        public LambdaExpression CreateMapExpression(ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            var instanceParameter = Expression.Parameter(request.SourceType, "dto");
            var total = CreateMapExpression(request, instanceParameter, typePairCount);
            if (total == null)
            {
                return null;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(request.SourceType, request.DestinationType);
            return Expression.Lambda(delegateType, total, instanceParameter);
        }

        public Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount)
        {
            var typeMap = _configurationProvider.ResolveTypeMap(request.SourceType,
                request.DestinationType);

            if (typeMap == null)
            {
                throw QueryMapperHelper.MissingMapException(request.SourceType, request.DestinationType);
            }

            if (typeMap.CustomProjection != null)
            {
                return typeMap.CustomProjection.ReplaceParameters(instanceParameter);
            }

            var bindings = new List<MemberBinding>();
            int depth = GetDepth(request, typePairCount);
            if (typeMap.MaxDepth > 0 && depth >= typeMap.MaxDepth)
            {
                if (typeMap.Profile.AllowNullDestinationValues)
                {
                    return null;
                }
            }
            else
            {
                bindings = CreateMemberBindings(request, typeMap, instanceParameter, typePairCount);
            }
            Expression constructorExpression = DestinationConstructorExpression(typeMap, instanceParameter);
            if (instanceParameter is ParameterExpression)
                constructorExpression = ((LambdaExpression)constructorExpression).ReplaceParameters(instanceParameter);
            var visitor = new NewFinderVisitor();
            visitor.Visit(constructorExpression);

            var expression = Expression.MemberInit(
                visitor.NewExpression,
                bindings.ToArray()
                );
            return expression;
        }

        private static int GetDepth(ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount)
        {
            int visitCount = 0;
            if (typePairCount.TryGetValue(request, out visitCount))
            {
                visitCount = visitCount + 1;
            }
            typePairCount[request] = visitCount;
            return visitCount;
        }

        private LambdaExpression DestinationConstructorExpression(TypeMap typeMap, Expression instanceParameter)
        {
            var ctorExpr = typeMap.ConstructExpression;
            if (ctorExpr != null)
            {
                return ctorExpr;
            }
            var newExpression = typeMap.ConstructorMap?.CanResolve == true
                ? typeMap.ConstructorMap.NewExpression(instanceParameter)
                : New(typeMap.DestinationTypeToUse);

            return Lambda(newExpression);
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
            Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount)
        {
            var bindings = new List<MemberBinding>();

            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var result = ResolveExpression(propertyMap, request.SourceType, instanceParameter);

                if (propertyMap.ExplicitExpansion &&
                    !request.MembersToExpand.Contains(propertyMap.DestinationProperty))
                    continue;

                var propertyTypeMap = _configurationProvider.ResolveTypeMap(result.Type,
                    propertyMap.DestinationPropertyType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationPropertyType, request.MembersToExpand, request);

                if (!propertyRequest.AlreadyExists)
                {
                    var binder = Binders.FirstOrDefault(b => b.IsMatch(propertyMap, propertyTypeMap, result));

                    if (binder == null)
                    {
                        var message =
                            $"Unable to create a map expression from {propertyMap.SourceMember?.DeclaringType?.Name}.{propertyMap.SourceMember?.Name} ({result.Type}) to {propertyMap.DestinationProperty.DeclaringType?.Name}.{propertyMap.DestinationProperty.Name} ({propertyMap.DestinationPropertyType})";

                        throw new AutoMapperMappingException(message, null, typeMap.Types, typeMap, propertyMap);
                    }

                    var bindExpression = binder.Build(_configurationProvider, propertyMap, propertyTypeMap,
                        propertyRequest, result, typePairCount);

                    if (bindExpression != null)
                    {
                        bindings.Add(bindExpression);
                    }
                }
            }
            return bindings;
        }

        private static ExpressionResolutionResult ResolveExpression(PropertyMap propertyMap, Type currentType,
            Expression instanceParameter)
        {
            var result = new ExpressionResolutionResult(instanceParameter, currentType);

            var matchingExpressionConverter =
                ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, propertyMap));
            if (matchingExpressionConverter == null)
                throw new Exception("Can't resolve this to Queryable Expression");
            result = matchingExpressionConverter.GetExpressionResolutionResult(result, propertyMap);

            if (propertyMap.NullSubstitute != null && result.Type.IsNullableType())
            {
                Expression currentChild = result.ResolutionExpression;
                Type currentChildType = result.Type;
                var nullSubstitute = propertyMap.NullSubstitute;

                var newParameter = result.ResolutionExpression;
                var converter = new NullSubstitutionConversionVisitor(newParameter, nullSubstitute);

                currentChild = converter.Visit(currentChild);
                currentChildType = currentChildType.GetTypeOfNullable();

                return new ExpressionResolutionResult(currentChild, currentChildType);
            }

            return result;
        }

        private class NullSubstitutionConversionVisitor : ExpressionVisitor
        {
            private readonly Expression _newParameter;
            private readonly object _nullSubstitute;

            public NullSubstitutionConversionVisitor(Expression newParameter, object nullSubstitute)
            {
                _newParameter = newParameter;
                _nullSubstitute = nullSubstitute;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                return node == _newParameter ? NullCheck(node) : node;
            }

            private Expression NullCheck(Expression input)
            {
                var underlyingType = input.Type.GetTypeOfNullable();
                var nullSubstitute = ExpressionExtensions.ToType(Constant(_nullSubstitute), underlyingType);
                return Condition(Property(input, "HasValue"), Property(input, "Value"), nullSubstitute, underlyingType);
            }
        }

        internal class ConstantExpressionReplacementVisitor : ExpressionVisitor
        {
            private readonly IDictionary<string, object> _paramValues;

            public ConstantExpressionReplacementVisitor(
                IDictionary<string, object> paramValues)
            {
                _paramValues = paramValues;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!node.Member.DeclaringType.Has<CompilerGeneratedAttribute>())
                {
                    return base.VisitMember(node);
                }
                object parameterValue;
                var parameterName = node.Member.Name;
                if (!_paramValues.TryGetValue(parameterName, out parameterValue))
                {
                    const string VBPrefix = "$VB$Local_";
                    if (!parameterName.StartsWith(VBPrefix, StringComparison.Ordinal) || !_paramValues.TryGetValue(parameterName.Substring(VBPrefix.Length), out parameterValue))
                    {
                        return base.VisitMember(node);
                    }
                }
                return Convert(Constant(parameterValue), node.Member.GetMemberType());
            }
        }

        /// <summary>
        /// Expression visitor for making member access null-safe.
        /// </summary>
        /// <remarks>
        /// Use <see cref="NullsafeQueryRewriter" /> to make a query null-safe.
        /// copied from NeinLinq (MIT License): https://github.com/axelheer/nein-linq/blob/master/src/NeinLinq/NullsafeQueryRewriter.cs
        /// </remarks>
        internal class NullsafeQueryRewriter : ExpressionVisitor
        {
            static readonly LockingConcurrentDictionary<Type, Expression> Cache = new LockingConcurrentDictionary<Type, Expression>(NodeFallback);

            /// <inheritdoc />
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node?.Expression != null)
                {
                    return MakeNullsafe(node, node.Expression);
                }

                return base.VisitMember(node);
            }

            /// <inheritdoc />
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node?.Object != null)
                {
                    return MakeNullsafe(node, node.Object);
                }

                return base.VisitMethodCall(node);
            }

            Expression MakeNullsafe(Expression node, Expression value)
            {
                // cache "fallback expression" for performance reasons
                var fallback = Cache.GetOrAdd(node.Type);

                // check value and insert additional coalesce, if fallback is not default
                return Expression.Condition(
                    Expression.NotEqual(Visit(value), Expression.Default(value.Type)),
                    fallback.NodeType != ExpressionType.Default ? Expression.Coalesce(node, fallback) : node,
                    fallback);
            }

            static Expression NodeFallback(Type type)
            {
                // default values for generic collections
                if (type.IsConstructedGenericType && type.GenericTypeArguments.Length == 1)
                {
                    return GenericCollectionFallback(typeof(List<>), type)
                        ?? GenericCollectionFallback(typeof(HashSet<>), type)
                        ?? Expression.Default(type);
                }

                // default value for arrays
                if (type.IsArray)
                {
                    return Expression.NewArrayInit(type.GetElementType());
                }

                // default value
                return Expression.Default(type);
            }

            static Expression GenericCollectionFallback(Type collectionDefinition, Type type)
            {
                var collectionType = collectionDefinition.MakeGenericType(type.GenericTypeArguments);

                // try if an instance of this collection would suffice
                if (type.GetTypeInfo().IsAssignableFrom(collectionType.GetTypeInfo()))
                {
                    return Expression.Convert(Expression.New(collectionType), type);
                }

                return null;
            }
        }
    }
}