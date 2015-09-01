namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Impl;
    using Internal;
    using Mappers;
    using QueryableExtensions;
    using QueryableExtensions.Impl;

    public class MappingEngine : IMappingEngine, IMappingEngineRunner
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private static readonly IProxyGeneratorFactory ProxyGeneratorFactory = PlatformAdapter.Resolve<IProxyGeneratorFactory>();

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


        private bool _disposed;
        private readonly IObjectMapper[] _mappers;
        private readonly Internal.IDictionary<TypePair, IObjectMapper> _objectMapperCache;
        private readonly Internal.IDictionary<ExpressionRequest, LambdaExpression> _expressionCache;

        private readonly Func<Type, object> _serviceCtor;

        public MappingEngine(IConfigurationProvider configurationProvider)
            : this(
                configurationProvider,
                DictionaryFactory.CreateDictionary<TypePair, IObjectMapper>(),
                DictionaryFactory.CreateDictionary<ExpressionRequest, LambdaExpression>(),
                configurationProvider.ServiceCtor)
        {
        }

        public MappingEngine(IConfigurationProvider configurationProvider, Internal.IDictionary<TypePair, IObjectMapper> objectMapperCache, Internal.IDictionary<ExpressionRequest, LambdaExpression> expressionCache,
            Func<Type, object> serviceCtor)
        {
            ConfigurationProvider = configurationProvider;
            _objectMapperCache = objectMapperCache;
            _expressionCache = expressionCache;
            _serviceCtor = serviceCtor;
            _mappers = configurationProvider.GetMappers();
            ConfigurationProvider.TypeMapCreated += ClearTypeMap;
        }

        public IConfigurationProvider ConfigurationProvider { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (ConfigurationProvider != null)
                        ConfigurationProvider.TypeMapCreated -= ClearTypeMap;
                }

                _disposed = true;
            }
        }

        public TDestination Map<TDestination>(object source)
        {
            return Map<TDestination>(source, DefaultMappingOptions);
        }

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            var mappedObject = default(TDestination);
            if (source != null)
            {
                var sourceType = source.GetType();
                var destinationType = typeof (TDestination);

                mappedObject = (TDestination) Map(source, sourceType, destinationType, opts);
            }
            return mappedObject;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            return (TDestination) Map(source, modelType, destinationType, DefaultMappingOptions);
        }

        public TDestination Map<TSource, TDestination>(TSource source,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination) MapCore(source, modelType, destinationType, options);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Map(source, destination, DefaultMappingOptions);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination) MapCore(source, destination, modelType, destinationType, options);
        }

        public object Map(object source, Type sourceType, Type destinationType)
        {
            return Map(source, sourceType, destinationType, DefaultMappingOptions);
        }

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, sourceType, destinationType, options);
        }

        private object MapCore(object source, Type sourceType, Type destinationType, MappingOperationOptions options)
        {
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType, options, this);

            return ((IMappingEngineRunner) this).Map(context);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return Map(source, destination, sourceType, destinationType, DefaultMappingOptions);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, destination, sourceType, destinationType, options);
        }

        private object MapCore(object source, object destination, Type sourceType, Type destinationType,
            MappingOperationOptions options)
        {
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, options, this);

            return ((IMappingEngineRunner) this).Map(context);
        }


        public TDestination DynamicMap<TSource, TDestination>(TSource source)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            return (TDestination) DynamicMap(source, modelType, destinationType);
        }

        public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            DynamicMap(source, destination, modelType, destinationType);
        }

        public TDestination DynamicMap<TDestination>(object source)
        {
            Type modelType = source?.GetType() ?? typeof (object);
            Type destinationType = typeof (TDestination);

            return (TDestination) DynamicMap(source, modelType, destinationType);
        }

        public object DynamicMap(object source, Type sourceType, Type destinationType)
        {
            var typeMap = ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType,
                new MappingOperationOptions
                {
                    CreateMissingTypeMaps = true
                }, this);

            return ((IMappingEngineRunner) this).Map(context);
        }

        public void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            var typeMap = ConfigurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType,
                new MappingOperationOptions
                {
                    CreateMissingTypeMaps = true
                }, this);

            ((IMappingEngineRunner) this).Map(context);
        }

        public TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source)
        {
            Type destinationType = typeof (TDestination);
            Type sourceType = typeof (TSource);
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);
            var context = parentContext.CreateTypeContext(typeMap, source, null, sourceType, destinationType);
            return (TDestination) ((IMappingEngineRunner) this).Map(context);
        }

        public Expression CreateMapExpression(Type sourceType, Type destinationType, System.Collections.Generic.IDictionary<string, object> parameters = null, params MemberInfo[] membersToExpand)
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var cachedExpression =
                _expressionCache.GetOrAdd(new ExpressionRequest(sourceType, destinationType, membersToExpand),
                    tp => CreateMapExpression(tp, DictionaryFactory.CreateDictionary<ExpressionRequest, int>()));

            if (!parameters.Any())
                return cachedExpression;

            var visitor = new ConstantExpressionReplacementVisitor(parameters);

            return visitor.Visit(cachedExpression);
        }

        public LambdaExpression CreateMapExpression(ExpressionRequest request, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            var instanceParameter = Expression.Parameter(request.SourceType, "dto");
            var total = CreateMapExpression(request, instanceParameter, typePairCount);
            var delegateType = typeof(Func<,>).MakeGenericType(request.SourceType, request.DestinationType);
            return Expression.Lambda(delegateType, total, instanceParameter);
        }

        public Expression CreateMapExpression(ExpressionRequest request,
            Expression instanceParameter, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var typeMap = ConfigurationProvider.ResolveTypeMap(request.SourceType,
                request.DestinationType);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, request.SourceType.Name, request.DestinationType.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(request, typeMap, instanceParameter, typePairCount);

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

        private List<MemberBinding> CreateMemberBindings(ExpressionRequest request,
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
                    !request.MembersToExpand.Contains(propertyMap.DestinationProperty.MemberInfo))
                    continue;

                var propertyTypeMap = ConfigurationProvider.ResolveTypeMap(result.Type,
                    propertyMap.DestinationPropertyType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationPropertyType, request.MembersToExpand);

                var binder = Binders.FirstOrDefault(b => b.IsMatch(propertyMap, propertyTypeMap, result));

                if (binder == null)
                {
                    var message =
                        $"Unable to create a map expression from {propertyMap.SourceMember?.DeclaringType?.Name}.{propertyMap.SourceMember?.Name} ({result.Type}) to {propertyMap.DestinationProperty.MemberInfo.DeclaringType?.Name}.{propertyMap.DestinationProperty.Name} ({propertyMap.DestinationPropertyType})";

                    throw new AutoMapperMappingException(message);
                }

                var bindExpression = binder.Build(this, propertyMap, propertyTypeMap, propertyRequest, result, typePairCount);

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


        object IMappingEngineRunner.Map(ResolutionContext context)
        {
            try
            {
                var contextTypePair = new TypePair(context.SourceType, context.DestinationType);

                Func<TypePair, IObjectMapper> missFunc =
                    tp => _mappers.FirstOrDefault(mapper => mapper.IsMatch(context));

                IObjectMapper mapperToUse = _objectMapperCache.GetOrAdd(contextTypePair, missFunc);
                if (mapperToUse == null || (context.Options.CreateMissingTypeMaps && !mapperToUse.IsMatch(context)))
                {
                    if (context.Options.CreateMissingTypeMaps)
                    {
                        var typeMap = ConfigurationProvider.CreateTypeMap(context.SourceType, context.DestinationType);
                        context = context.CreateTypeContext(typeMap, context.SourceValue, context.DestinationValue, context.SourceType, context.DestinationType);
                        mapperToUse = missFunc(contextTypePair);
                        if(mapperToUse == null)
                        {
                            throw new AutoMapperMappingException(context, "Unsupported mapping.");
                        }
                        _objectMapperCache.AddOrUpdate(contextTypePair, mapperToUse, (tp, mapper) => mapperToUse);
                    }
                    else
                    {
                        if(context.SourceValue != null)
                        {
                            throw new AutoMapperMappingException(context, "Missing type map configuration or unsupported mapping.");
                        }
                        return ObjectCreator.CreateDefaultValue(context.DestinationType);
                    }
                }

                return mapperToUse.Map(context, this);
            }
            catch (AutoMapperMappingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AutoMapperMappingException(context, ex);
            }
        }

        object IMappingEngineRunner.CreateObject(ResolutionContext context)
        {
            var typeMap = context.TypeMap;
            var destinationType = context.DestinationType;

            if (typeMap != null)
                if (typeMap.DestinationCtor != null)
                    return typeMap.DestinationCtor(context);
                else if (typeMap.ConstructDestinationUsingServiceLocator)
                    return context.Options.ServiceCtor(destinationType);
                else if (typeMap.ConstructorMap != null && typeMap.ConstructorMap.CtorParams.All(p => p.CanResolve))
                    return typeMap.ConstructorMap.ResolveValue(context, this);

            if (context.DestinationValue != null)
                return context.DestinationValue;

            if (destinationType.IsInterface())
                destinationType = ProxyGeneratorFactory.Create().GetProxyType(destinationType);

            return !ConfigurationProvider.MapNullSourceValuesAsNull
                ? ObjectCreator.CreateNonNullValue(destinationType)
                : ObjectCreator.CreateObject(destinationType);
        }

        bool IMappingEngineRunner.ShouldMapSourceValueAsNull(ResolutionContext context)
        {
            if (context.DestinationType.IsValueType() && !context.DestinationType.IsNullableType())
                return false;

            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
                return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceValuesAsNull;

            return ConfigurationProvider.MapNullSourceValuesAsNull;
        }

        bool IMappingEngineRunner.ShouldMapSourceCollectionAsNull(ResolutionContext context)
        {
            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
                return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceCollectionsAsNull;

            return ConfigurationProvider.MapNullSourceCollectionsAsNull;
        }

        private void ClearTypeMap(object sender, TypeMapCreatedEventArgs e)
        {
            IObjectMapper existing;

            _objectMapperCache.TryRemove(new TypePair(e.TypeMap.SourceType, e.TypeMap.DestinationType), out existing);
        }

        private void DefaultMappingOptions(IMappingOperationOptions opts)
        {
            opts.ConstructServicesUsing(_serviceCtor);
        }
    }
}