namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Impl;
    using Internal;

    public static class Extensions
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private static readonly Internal.IDictionary<ExpressionRequest, LambdaExpression> _expressionCache
            = DictionaryFactory.CreateDictionary<ExpressionRequest, LambdaExpression>();

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

        public static void ClearExpressionCache()
        {
            _expressionCache.Clear();
        }

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
            this IMappingEngine mappingEngine, System.Collections.Generic.IDictionary<string, object> parameters = null,
            params string[] membersToExpand)
        {
            return
                (Expression<Func<TSource, TDestination>>)
                    mappingEngine.CreateMapExpression(typeof (TSource), typeof (TDestination), parameters,
                        membersToExpand);
        }

        /// <summary>
        /// Create an expression tree representing a mapping from the source type to destination type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <param name="sourceType">Source Type</param>
        /// <param name="destinationType">Destination Type</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Expand members explicitly previously marked as members to explicitly expand</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression CreateMapExpression(this IMappingEngine mappingEngine,
            Type sourceType, Type destinationType,
            System.Collections.Generic.IDictionary<string, object> parameters = null, params string[] membersToExpand)
        {
            //Expression.const
            parameters = parameters ?? new Dictionary<string, object>();

            var cachedExpression =
                _expressionCache.GetOrAdd(new ExpressionRequest(sourceType, destinationType, membersToExpand),
                    tp =>
                        CreateMapExpression(mappingEngine, tp,
                            DictionaryFactory.CreateDictionary<ExpressionRequest, int>()));

            if (!parameters.Any())
                return cachedExpression;

            var visitor = new ConstantExpressionReplacementVisitor(parameters);

            return visitor.Visit(cachedExpression);
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
            return new ProjectionExpression(source, mappingEngine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the static <see cref="Mapper.Engine"/> property.
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source)
        {
            return source.ProjectTo<TDestination>(Mapper.Engine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the provided mapping engine
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <returns>Expression to project into</returns>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source, IMappingEngine mappingEngine)
        {
            return new ProjectionExpression(source, mappingEngine).To<TDestination>();
        }

        internal static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, ExpressionRequest request,
            Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(request.SourceType, "dto");

            var total = CreateMapExpression(mappingEngine, request, instanceParameter, typePairCount);

            return Expression.Lambda(total, instanceParameter);
        }

        internal static Expression CreateMapExpression(IMappingEngine mappingEngine, ExpressionRequest request,
            Expression instanceParameter, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var typeMap = mappingEngine.ConfigurationProvider.ResolveTypeMap(request.SourceType,
                request.DestinationType);

            if(typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, request.SourceType.Name, request.DestinationType.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(mappingEngine, request, typeMap, instanceParameter, typePairCount);

            var parameterReplacer = new ParameterReplacementVisitor(instanceParameter);
            var visitor = new NewFinderVisitor();
            var constructorExpression = typeMap.DestinationConstructorExpression(instanceParameter);
            visitor.Visit(parameterReplacer.Visit(constructorExpression));

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

                if (propertyMap.ExplicitExpansion &&
                    !request.IncludedMembers.Contains(propertyMap.DestinationProperty.Name))
                    continue;

                var propertyTypeMap = mappingEngine.ConfigurationProvider.ResolveTypeMap(result.Type,
                    propertyMap.DestinationPropertyType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationPropertyType,
                    request.IncludedMembers);

                var binder = Binders.FirstOrDefault(b => b.IsMatch(propertyMap, propertyTypeMap, result));

                if (binder == null)
                {
                    var message =
                        $"Unable to create a map expression from {propertyMap.SourceMember.DeclaringType.Name}.{propertyMap.SourceMember.Name} ({result.Type}) to {propertyMap.DestinationProperty.MemberInfo.DeclaringType.Name}.{propertyMap.DestinationProperty.Name} ({propertyMap.DestinationPropertyType})";

                    throw new AutoMapperMappingException(message);
                }

                var bindExpression = binder.Build(mappingEngine, propertyMap, propertyTypeMap, propertyRequest, result,
                    typePairCount);

                bindings.Add(bindExpression);
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
            private readonly System.Collections.Generic.IDictionary<string, object> _paramValues;

            public ConstantExpressionReplacementVisitor(
                System.Collections.Generic.IDictionary<string, object> paramValues)
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