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
    using TypePairCount = IDictionary<ExpressionRequest, int>;
    using ParameterBag = IDictionary<string, object>;

    public interface IExpressionBuilder
    {
        LambdaExpression[] GetMapExpression(Type sourceType, Type destinationType, ParameterBag parameters, MemberInfo[] membersToExpand);
        LambdaExpression[] CreateMapExpression(ExpressionRequest request, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps);
        Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps);
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

        public LambdaExpression[] GetMapExpression(Type sourceType, Type destinationType, ParameterBag parameters,
            MemberInfo[] membersToExpand)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (membersToExpand == null)
            {
                throw new ArgumentNullException(nameof(membersToExpand));
            }

            var cachedExpressions = _expressionCache.GetOrAdd(new ExpressionRequest(sourceType, destinationType, membersToExpand, null));

            return cachedExpressions.Select(e => Prepare(e, parameters)).Cast<LambdaExpression>().ToArray();
        }

        private Expression Prepare(Expression cachedExpression, ParameterBag parameters)
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

        private LambdaExpression[] CreateMapExpression(ExpressionRequest request) => CreateMapExpression(request, new Dictionary<ExpressionRequest, int>(), new FirstPassLetPropertyMaps(_configurationProvider));

        public LambdaExpression[] CreateMapExpression(ExpressionRequest request, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps)
        {
            // this is the input parameter of this expression with name <variableName>
            var instanceParameter = Parameter(request.SourceType, "dto");
            var expressions = new QueryExpressions(CreateMapExpressionCore(request, instanceParameter, typePairCount, letPropertyMaps, out var typeMap));
            if(letPropertyMaps.Count > 0)
            {
                expressions = letPropertyMaps.GetSubQueryExpression(this, expressions.First, typeMap, request, instanceParameter, typePairCount);
            }
            if(expressions.First == null)
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

        public Expression CreateMapExpression(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps)
        {
            return CreateMapExpressionCore(request, instanceParameter, typePairCount, letPropertyMaps, out var _);
        }

        private Expression CreateMapExpressionCore(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps, out TypeMap typeMap)
        {
            typeMap = _configurationProvider.ResolveTypeMap(request.SourceType, request.DestinationType);

            if(typeMap == null)
            {
                throw QueryMapperHelper.MissingMapException(request.SourceType, request.DestinationType);
            }

            if(typeMap.CustomProjection != null)
            {
                return typeMap.CustomProjection.ReplaceParameters(instanceParameter);
            }
            return CreateMapExpressionCore(request, instanceParameter, typePairCount, typeMap, letPropertyMaps);
        }

        private Expression CreateMapExpressionCore(ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount, TypeMap typeMap, LetPropertyMaps letPropertyMaps)
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
                bindings = CreateMemberBindings(request, typeMap, instanceParameter, typePairCount, letPropertyMaps);
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

        private static int GetDepth(ExpressionRequest request, TypePairCount typePairCount)
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
            Expression instanceParameter, TypePairCount typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var bindings = new List<MemberBinding>();

            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue() && ReflectionHelper.CanBeSet(pm.DestinationProperty)))
            {
                letPropertyMaps.Push(propertyMap);

                CreateMemberBinding(propertyMap);

                letPropertyMaps.Pop();
            }
            return bindings;
            void CreateMemberBinding(PropertyMap propertyMap)
            {
                var result = ResolveExpression(propertyMap, request.SourceType, instanceParameter, letPropertyMaps);

                if(propertyMap.ExplicitExpansion && !request.MembersToExpand.Contains(propertyMap.DestinationProperty))
                {
                    return;
                }
                var propertyTypeMap = _configurationProvider.ResolveTypeMap(result.Type,
                    propertyMap.DestinationPropertyType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationPropertyType, request.MembersToExpand, request);

                if(!propertyRequest.AlreadyExists)
                {
                    var binder = Binders.FirstOrDefault(b => b.IsMatch(propertyMap, propertyTypeMap, result));

                    if(binder == null)
                    {
                        var message =
                            $"Unable to create a map expression from {propertyMap.SourceMember?.DeclaringType?.Name}.{propertyMap.SourceMember?.Name} ({result.Type}) to {propertyMap.DestinationProperty.DeclaringType?.Name}.{propertyMap.DestinationProperty.Name} ({propertyMap.DestinationPropertyType})";

                        throw new AutoMapperMappingException(message, null, typeMap.Types, typeMap, propertyMap);
                    }

                    var bindExpression = binder.Build(_configurationProvider, propertyMap, propertyTypeMap,
                        propertyRequest, result, typePairCount, letPropertyMaps);


                    if (bindExpression != null)
                    {
                        var rhs = propertyMap.ValueTransformers
                            .Concat(typeMap.ValueTransformers)
                            .Concat(typeMap.Profile.ValueTransformers)
                            .Where(vt => vt.IsMatch(propertyMap))
                            .Aggregate(bindExpression.Expression, (current, vtConfig) => ToType(ReplaceParameters(vtConfig.TransformerExpression, ToType(current, vtConfig.ValueType)), propertyMap.DestinationPropertyType));

                        bindExpression = bindExpression.Update(rhs);

                        bindings.Add(bindExpression);
                    }
                }
            }
        }

        private static ExpressionResolutionResult ResolveExpression(PropertyMap propertyMap, Type currentType,
            Expression instanceParameter, LetPropertyMaps letPropertyMaps)
        {
            var result = new ExpressionResolutionResult(instanceParameter, currentType);

            var matchingExpressionConverter =
                ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, propertyMap));
            result = matchingExpressionConverter?.GetExpressionResolutionResult(result, propertyMap, letPropertyMaps) 
                ?? throw new Exception("Can't resolve this to Queryable Expression");

            if(propertyMap.NullSubstitute != null && result.Type.IsNullableType())
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
            private readonly ParameterBag _paramValues;

            public ConstantExpressionReplacementVisitor(
                ParameterBag paramValues) => _paramValues = paramValues;

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
                if (type.GetIsConstructedGenericType() && type.GetTypeInfo().GenericTypeArguments.Length == 1)
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
                var collectionType = collectionDefinition.MakeGenericType(type.GetTypeInfo().GenericTypeArguments);

                // try if an instance of this collection would suffice
                return type.GetTypeInfo().IsAssignableFrom(collectionType.GetTypeInfo()) ? Convert(New(collectionType), type) : null;
            }
        }


        public class FirstPassLetPropertyMaps : LetPropertyMaps
        {
            Stack<PropertyMap> _currentPath = new Stack<PropertyMap>();
            List<PropertyPath> _savedPaths = new List<PropertyPath>();
            IConfigurationProvider _configurationProvider;

            public FirstPassLetPropertyMaps(IConfigurationProvider configurationProvider) =>
                _configurationProvider = configurationProvider;

            public override Expression GetSubQueryMarker()
            {
                var propertyMap = _currentPath.Peek();
                var mapFrom = propertyMap.CustomExpression;
                if(!IsSubQuery() || _configurationProvider.ResolveTypeMap(propertyMap.SourceType, propertyMap.DestinationPropertyType) == null)
                {
                    return null;
                }
                var type = mapFrom.Body.Type;
                var marker = Parameter(type, "marker" + propertyMap.DestinationProperty.Name);
                _savedPaths.Add(new PropertyPath(_currentPath.Reverse().ToArray(), marker));
                return marker;
                bool IsSubQuery()
                {
                    if(!(mapFrom.Body is MethodCallExpression methodCall))
                    {
                        return false;
                    }
                    var method = methodCall.Method;
                    return method.IsStatic && method.DeclaringType == typeof(Enumerable);
                }
            }

            public override void Push(PropertyMap propertyMap) => _currentPath.Push(propertyMap);

            public override void Pop() => _currentPath.Pop();

            public override int Count => _savedPaths.Count;

            public override LetPropertyMaps New() => new FirstPassLetPropertyMaps(_configurationProvider);

            public override QueryExpressions GetSubQueryExpression(ExpressionBuilder builder, Expression projection, TypeMap typeMap, ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount)
            {
                var letMapInfos = _savedPaths.Select(path => new
                {
                    MapFrom = path.Last.CustomExpression,
                    MapFromSource = path.PropertyMaps.Take(path.PropertyMaps.Length - 1).Select(pm=>pm.SourceMember).MemberAccesses(instanceParameter),
                    Property = new PropertyDescription
                    (
                        string.Join("_", path.PropertyMaps.Select(pm => pm.DestinationProperty.Name)),
                        path.Last.SourceType
                    ),
                    Marker = path.Marker
                }).ToArray();

                var letProperties = letMapInfos.Select(m => m.Property);
                var letType = ProxyGenerator.GetSimilarType(request.SourceType, letProperties);
                var typeMapFactory = new TypeMapFactory();
                TypeMap firstTypeMap;
                lock(_configurationProvider)
                {
                    firstTypeMap = typeMapFactory.CreateTypeMap(request.SourceType, letType, typeMap.Profile, MemberList.None);
                }
                var secondParameter = Parameter(letType, "dto");

                ReplaceSubQueries();

                var firstExpression = builder.CreateMapExpressionCore(request, instanceParameter, typePairCount, firstTypeMap, LetPropertyMaps.Default);
                return new QueryExpressions(firstExpression, projection, secondParameter);

                void ReplaceSubQueries()
                {
                    foreach(var letMapInfo in letMapInfos)
                    {
                        var letProperty = letType.GetDeclaredProperty(letMapInfo.Property.Name);
                        var letPropertyMap = firstTypeMap.FindOrCreatePropertyMapFor(letProperty);
                        letPropertyMap.CustomExpression =
                            Lambda(letMapInfo.MapFrom.ReplaceParameters(letMapInfo.MapFromSource), (ParameterExpression)instanceParameter);
                        projection = projection.Replace(letMapInfo.Marker, MakeMemberAccess(secondParameter, letProperty));
                    }
                    projection = new ReplaceMemberAccessesVisitor(instanceParameter, secondParameter).Visit(projection);
                }
            }

            class ReplaceMemberAccessesVisitor : ExpressionVisitor
            {
                Expression _oldObject, _newObject;

                public ReplaceMemberAccessesVisitor(Expression oldObject, Expression newObject)
                {
                    _oldObject = oldObject;
                    _newObject = newObject;
                }

                protected override Expression VisitMember(MemberExpression node)
                {
                    if(node.Expression != _oldObject)
                    {
                        return base.VisitMember(node);
                    }
                    return MakeMemberAccess(_newObject, _newObject.Type.GetFieldOrProperty(node.Member.Name));
                }
            }
        }
    }

    public class LetPropertyMaps
    {
        public static readonly LetPropertyMaps Default = new LetPropertyMaps();

        protected LetPropertyMaps() { }

        public virtual Expression GetSubQueryMarker() => null;

        public virtual void Push(PropertyMap propertyMap) {}

        public virtual void Pop() {}

        public virtual int Count => 0;

        public virtual LetPropertyMaps New() => Default;

        public virtual QueryExpressions GetSubQueryExpression(ExpressionBuilder builder, Expression projection, TypeMap typeMap, ExpressionRequest request, Expression instanceParameter, TypePairCount typePairCount)
            => throw new NotImplementedException();

        public struct PropertyPath
        {
            public PropertyPath(PropertyMap[] propertyMaps, Expression marker)
            {
                PropertyMaps = propertyMaps;
                Marker = marker;
            }
            public PropertyMap[] PropertyMaps { get; }
            public Expression Marker { get; }
            public PropertyMap Last => PropertyMaps[PropertyMaps.Length - 1];
        }
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

    public static class ExpressionBuilderExtensions
    {
        public static Expression<Func<TSource, TDestination>> GetMapExpression<TSource, TDestination>(
            this IExpressionBuilder expressionBuilder)
        {
            return (Expression<Func<TSource, TDestination>>) expressionBuilder.GetMapExpression(typeof(TSource),
                typeof(TDestination), new Dictionary<string, object>(), new MemberInfo[0])[0];
        }
    }
}