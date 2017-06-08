using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Configuration;
using AutoMapper.Internal;
using AutoMapper.Execution;
using AutoMapper.QueryableExtensions.Impl;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.QueryableExtensions
{
    using static ExpressionFactory;

    public interface IExpressionBuilder
    {
        LambdaExpression[] GetMapExpression(Type sourceType, Type destinationType, IDictionary<string, object> parameters, MemberInfo[] membersToExpand);
        Expression<Func<TSource, TDestination>> GetMapExpression<TSource, TDestination>(IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand);
        LambdaExpression[] CreateMapExpression(ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount);
        Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount);
    }

    public class ExpressionBuilder : IExpressionBuilder
    {
        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberResolverExpressionResultConverter(),
            new MemberGetterExpressionResultConverter()
        };

        private static readonly IExpressionBinder[] Binders =
        {
            new CustomProjectionExpressionBinder(),
            new NullableDestinationExpressionBinder(),
            new NullableSourceExpressionBinder(),
            new AssignableExpressionBinder(),
            new EnumerableExpressionBinder(),
            new MappedTypeExpressionBinder(),
            new StringExpressionBinder()
        };

        private readonly LockingConcurrentDictionary<ExpressionRequest, LambdaExpression[]> _expressionCache;
        private readonly IConfigurationProvider _configurationProvider;

        public ExpressionBuilder(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
            _expressionCache = new LockingConcurrentDictionary<ExpressionRequest, LambdaExpression[]>(CreateMapExpression);
        }

        public LambdaExpression[] GetMapExpression(Type sourceType, Type destinationType, IDictionary<string, object> parameters, MemberInfo[] membersToExpand)
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var cachedExpressions = _expressionCache.GetOrAdd(new ExpressionRequest(sourceType, destinationType, membersToExpand, null));

            return cachedExpressions.Select(e => Prepare(e, parameters)).Cast<LambdaExpression>().ToArray();
        }

        private Expression Prepare(Expression cachedExpression, IDictionary<string, object> parameters)
        {
            Expression result;
            if(parameters.Any())
            {
                var visitor = new ConstantExpressionReplacementVisitor(parameters);
                result = visitor.Visit(cachedExpression);
            }
            else
            {
                result = cachedExpression;
            }
            // perform null-propagation if this feature is enabled.
            if(_configurationProvider.EnableNullPropagationForQueryMapping)
            {
                var nullVisitor = new NullsafeQueryRewriter();
                return nullVisitor.Visit(result);
            }
            return result;
        }

        public Expression<Func<TSource, TDestination>> GetMapExpression<TSource, TDestination>(
            IDictionary<string, object> parameters = null,
            params MemberInfo[] membersToExpand) => 
            (Expression<Func<TSource, TDestination>>)GetMapExpression(typeof(TSource), typeof(TDestination), parameters, membersToExpand)[0];

        private LambdaExpression[] CreateMapExpression(ExpressionRequest request) => CreateMapExpression(request, new Dictionary<ExpressionRequest, int>());

        public LambdaExpression[] CreateMapExpression(ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            var instanceParameter = Parameter(request.SourceType, "dto");
            var expressions = CreateMapExpressionCore(request, instanceParameter, typePairCount);
            if (expressions.First == null)
            {
                return null;
            }
            var firstLambda = Lambda(expressions.First, instanceParameter);
            if(expressions.Second == null)
            {
                return new[] { firstLambda };
            }
            return new[] { firstLambda, Lambda(expressions.Second, expressions.SecondParameter) };
        }

#if NET45
        private QueryExpressions? GetSubQueryExpression(TypeMap typeMap, ExpressionRequest request, Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount)
        {
            var subQueryProperties = GetSubQueryProperties();
            if(subQueryProperties.Length == 0)
            {
                return null;
            }
            var additionalProperties = subQueryProperties.Select(pm => new PropertyDescription(pm.DestinationProperty.Name, pm.SourceType));
            var letType = ProxyGenerator.GetSimilarType(request.SourceType, additionalProperties);
            var typeMapFactory = new TypeMapFactory();
            var firstTypeMap = typeMapFactory.CreateTypeMap(request.SourceType, letType, typeMap.Profile, MemberList.None);
            var secondTypeMap = typeMapFactory.CreateTypeMap(letType, request.DestinationType, typeMap.Profile, MemberList.None);
            
            ConfigureTheTypeMaps();

            var firstExpression = CreateMapExpressionCore(request, instanceParameter, typePairCount, firstTypeMap);
            var secondParameter = Parameter(letType, "dto");
            var secondExpression = CreateMapExpressionCore(request, secondParameter, typePairCount, secondTypeMap);
            
            return new QueryExpressions(firstExpression, secondExpression, secondParameter);
            
            PropertyMap[] GetSubQueryProperties()
            {
                return
                    (from propertyMap in typeMap.GetPropertyMaps()
                     where propertyMap.SourceType != null
                     let propertyTypeMap = _configurationProvider.ResolveTypeMap(propertyMap.SourceType, propertyMap.DestinationPropertyType)
                     where propertyMap.IsSubQuery() && propertyTypeMap != null && propertyTypeMap.CustomProjection == null
                     select propertyMap).ToArray();
            }
            
            void ConfigureTheTypeMaps()
            {
                secondTypeMap.ApplyInheritedTypeMap(typeMap);
                foreach(var subQueryPropertyMap in subQueryProperties)
                {
                    var secondPropertyMap = secondTypeMap.GetExistingPropertyMapFor(subQueryPropertyMap.DestinationProperty);
                    var subQuery = secondPropertyMap.CustomExpression;
                    secondPropertyMap.CustomExpression = null;
                    var firstPropertyMap = firstTypeMap.FindOrCreatePropertyMapFor(secondPropertyMap.SourceMember);
                    firstPropertyMap.CustomExpression = subQuery;
                }
            }
        }
#endif

        public Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount)
        {
            var queryExpressions = CreateMapExpressionCore(request, instanceParameter, typePairCount);
            return queryExpressions.First;
        }

        private QueryExpressions CreateMapExpressionCore(ExpressionRequest request, Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount)
        {
            var typeMap = _configurationProvider.ResolveTypeMap(request.SourceType, request.DestinationType);

            if(typeMap == null)
            {
                throw QueryMapperHelper.MissingMapException(request.SourceType, request.DestinationType);
            }

            if(typeMap.CustomProjection != null)
            {
                return new QueryExpressions(typeMap.CustomProjection.ReplaceParameters(instanceParameter));
            }
#if NET45
            var subQueryExpressions = GetSubQueryExpression(typeMap, request, instanceParameter, typePairCount);
            if(subQueryExpressions != null)
            {
                return subQueryExpressions.Value;
            }
#endif
            return new QueryExpressions(CreateMapExpressionCore(request, instanceParameter, typePairCount, typeMap));
        }

        private Expression CreateMapExpressionCore(ExpressionRequest request, Expression instanceParameter, IDictionary<ExpressionRequest, int> typePairCount, TypeMap typeMap)
        {
            var bindings = new List<MemberBinding>();
            var depth = GetDepth(request, typePairCount);
            if(typeMap.MaxDepth > 0 && depth >= typeMap.MaxDepth)
            {
                if(typeMap.Profile.AllowNullDestinationValues)
                {
                    return null;
                }
            }
            else
            {
                bindings = CreateMemberBindings(request, typeMap, instanceParameter, typePairCount);
            }
            Expression constructorExpression = DestinationConstructorExpression(typeMap, instanceParameter);
            if(instanceParameter is ParameterExpression)
                constructorExpression = ((LambdaExpression)constructorExpression).ReplaceParameters(instanceParameter);
            var visitor = new NewFinderVisitor();
            visitor.Visit(constructorExpression);

            var expression = MemberInit(
                visitor.NewExpression,
                bindings.ToArray()
                );
            return expression;
        }

        private static int GetDepth(ExpressionRequest request, IDictionary<ExpressionRequest, int> typePairCount)
        {
            if (typePairCount.TryGetValue(request, out int visitCount))
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

        public struct QueryExpressions
        {
            public QueryExpressions(Expression first, Expression second = null, ParameterExpression secondParameter = null)
            {
                First = first;
                Second = second;
                SecondParameter = secondParameter;
            }
            public Expression First { get; }
            public Expression Second { get; }
            public ParameterExpression SecondParameter { get; }
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
            result = matchingExpressionConverter?.GetExpressionResolutionResult(result, propertyMap) 
                ?? throw new Exception("Can't resolve this to Queryable Expression");

            if (propertyMap.NullSubstitute != null && result.Type.IsNullableType())
            {
                var currentChild = result.ResolutionExpression;
                var currentChildType = result.Type;
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

            protected override Expression VisitMember(MemberExpression node) => node == _newParameter ? NullCheck(node) : node;

            private Expression NullCheck(Expression input)
            {
                var underlyingType = input.Type.GetTypeOfNullable();
                var nullSubstitute = ToType(Constant(_nullSubstitute), underlyingType);
                return Condition(Property(input, "HasValue"), Property(input, "Value"), nullSubstitute, underlyingType);
            }
        }

        internal class ConstantExpressionReplacementVisitor : ExpressionVisitor
        {
            private readonly IDictionary<string, object> _paramValues;

            public ConstantExpressionReplacementVisitor(
                IDictionary<string, object> paramValues) => _paramValues = paramValues;

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!node.Member.DeclaringType.Has<CompilerGeneratedAttribute>())
                {
                    return base.VisitMember(node);
                }
                var parameterName = node.Member.Name;
                if (!_paramValues.TryGetValue(parameterName, out object parameterValue))
                {
                    const string vbPrefix = "$VB$Local_";
                    if (!parameterName.StartsWith(vbPrefix, StringComparison.Ordinal) || !_paramValues.TryGetValue(parameterName.Substring(vbPrefix.Length), out parameterValue))
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
            protected override Expression VisitMember(MemberExpression node) => 
                node?.Expression != null
                    ? MakeNullsafe(node, node.Expression)
                    : base.VisitMember(node);

            /// <inheritdoc />
            protected override Expression VisitMethodCall(MethodCallExpression node) => 
                node?.Object != null 
                    ? MakeNullsafe(node, node.Object) 
                    : base.VisitMethodCall(node);

            private Expression MakeNullsafe(Expression node, Expression value)
            {
                // cache "fallback expression" for performance reasons
                var fallback = Cache.GetOrAdd(node.Type);

                // check value and insert additional coalesce, if fallback is not default
                return Condition(
                    NotEqual(Visit(value), Default(value.Type)),
                    fallback.NodeType != ExpressionType.Default ? Coalesce(node, fallback) : node,
                    fallback);
            }

            private static Expression NodeFallback(Type type)
            {
                // default values for generic collections
                if (type.IsConstructedGenericType && type.GenericTypeArguments.Length == 1)
                {
                    return GenericCollectionFallback(typeof(List<>), type)
                        ?? GenericCollectionFallback(typeof(HashSet<>), type)
                        ?? Default(type);
                }

                // default value for arrays
                if (type.IsArray)
                {
                    return NewArrayInit(type.GetElementType());
                }

                // default value
                return Default(type);
            }

            private static Expression GenericCollectionFallback(Type collectionDefinition, Type type)
            {
                var collectionType = collectionDefinition.MakeGenericType(type.GenericTypeArguments);

                // try if an instance of this collection would suffice
                return type.GetTypeInfo().IsAssignableFrom(collectionType.GetTypeInfo()) ? Convert(New(collectionType), type) : null;
            }
        }
    }
}