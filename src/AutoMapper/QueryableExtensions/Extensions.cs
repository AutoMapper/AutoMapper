namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoMapper.Impl;
    using Impl;
    using Internal;

    public static class Extensions
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private static readonly Internal.IDictionary<ExpressionRequest, LambdaExpression> _expressionCache
            = DictionaryFactory.CreateDictionary<ExpressionRequest, LambdaExpression>();

        /// <summary>
        /// Create an expression tree representing a mapping from the <typeparamref name="TSource"/> type to <typeparamref name="TDestination"/> type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TDestination">Destination Type</typeparam>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Expand members explicitly previously marked as members to explicitly expand</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(
            this IMappingEngine mappingEngine, System.Collections.Generic.IDictionary<string, object> parameters = null, params string[] membersToExpand)
        {
            //Expression.const
            parameters = parameters ?? new Dictionary<string, object>();

            var cachedExpression = (Expression<Func<TSource, TDestination>>)
                _expressionCache.GetOrAdd(new ExpressionRequest(typeof(TSource), typeof(TDestination), membersToExpand),
                    tp => CreateMapExpression(mappingEngine, tp, DictionaryFactory.CreateDictionary<ExpressionRequest, int>()));

            if (!parameters.Any())
                return cachedExpression;

            var visitor = new ConstantExpressionReplacementVisitor(parameters);
            return (Expression<Func<TSource, TDestination>>)visitor.Visit(cachedExpression);
        }


        /// <summary>
        /// Extention method to project from a queryable using the static <see cref="Mapper.Engine"/> property.
        /// Due to generic parameter inference, you need to call Project().To to execute the map
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <returns>Expression to project into</returns>
        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source)
        {
            return source.Project(Mapper.Engine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the provided mapping engine
        /// Due to generic parameter inference, you need to call Project().To to execute the map
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <returns>Expression to project into</returns>
        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source, IMappingEngine mappingEngine)
        {
            return new ProjectionExpression<TSource>(source, mappingEngine);
        }

        internal static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, ExpressionRequest request,
            Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(request.SourceType, "dto");

            var total = CreateMapExpression(mappingEngine, request, instanceParameter, typePairCount);

            return Expression.Lambda(total, instanceParameter);
        }

        internal static Expression CreateMapExpression(IMappingEngine mappingEngine, ExpressionRequest request, Expression instanceParameter, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(request.SourceType,
                request.DestinationType);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, request.SourceType.Name, request.DestinationType.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(mappingEngine, request, typeMap, instanceParameter, typePairCount);

            var parameterReplacer = new ParameterReplacementVisitor(instanceParameter);
            var visitor = new NewFinderVisitor();
            var ctorExpr = typeMap.ConstructExpression ?? Expression.Lambda(Expression.New(request.DestinationType));
            visitor.Visit(parameterReplacer.Visit(ctorExpr));

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

        private static List<MemberBinding> CreateMemberBindings(IMappingEngine mappingEngine, ExpressionRequest request,
            TypeMap typeMap,
            Expression instanceParameter, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var bindings = new List<MemberBinding>();

            var visitCount = typePairCount.AddOrUpdate(request, 0, (tp, i) => i + 1);

            if (visitCount >= typeMap.MaxDepth)
                return bindings;

            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var result = ResolveExpression(propertyMap, request.SourceType, instanceParameter);

                if (propertyMap.ExplicitExpansion && !request.IncludedMembers.Contains(propertyMap.DestinationProperty.Name))
                    continue;

                var propertyTypeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(result.Type, propertyMap.DestinationPropertyType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationPropertyType, request.IncludedMembers);

                var binders = new IExpressionBinder[]
                {
                    new NullableExpressionBinder(),
                    new AssignableExpressionBinder(),
                    new EnumerableExpressionBinder(),
                    new MappedTypeExpressionBinder(),
                    new CustomProjectionExpressionBinder(), 
                    new StringExpressionBinder(),
                };

                var binder = binders.FirstOrDefault(b => b.IsMatch(propertyMap, propertyTypeMap, result));

                if (binder == null)
                    throw new AutoMapperMappingException("Unable to create a map expression from " + result.Type + " to " + propertyMap.DestinationPropertyType);

                var bindExpression = binder.Build(mappingEngine, propertyMap, propertyTypeMap, propertyRequest, result, typePairCount);

                bindings.Add(bindExpression);
            }
            return bindings;
        }

        private static readonly IList<IExpressionResultConverter> ExpressionResultConverters =
            new IExpressionResultConverter[]
            {
                new MemberGetterExpressionResultConverter(),
                new MemberResolverExpressionResultConverter(),
                new NullSubstitutionExpressionResultConverter(), 
            };

        private static ExpressionResolutionResult ResolveExpression(PropertyMap propertyMap, Type currentType, Expression instanceParameter)
        {
            var result = new ExpressionResolutionResult(instanceParameter, currentType);
            foreach (var resolver in propertyMap.GetSourceValueResolvers())
            {
                var matchingExpressionConverter = ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, resolver));
                if (matchingExpressionConverter == null)
                    throw new Exception("Can't resolve this to Queryable Expression");
                result = matchingExpressionConverter.GetExpressionResolutionResult(result, propertyMap, resolver);
            }
            return result;
        }

        private class ConstantExpressionReplacementVisitor : ExpressionVisitor
        {
            private readonly System.Collections.Generic.IDictionary<string, object> _paramValues;

            public ConstantExpressionReplacementVisitor(System.Collections.Generic.IDictionary<string, object> paramValues)
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

    internal class ParameterReplacementVisitor : ExpressionVisitor
    {
        private readonly Expression _memberExpression;

        public ParameterReplacementVisitor(Expression memberExpression)
        {
            _memberExpression = memberExpression;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _memberExpression;
        }
    }
}