using System;
#if !SILVERLIGHT
using System.Collections.Concurrent;
#else
using TvdP.Collections;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Mappers;

namespace AutoMapper
{
	public class MappingEngine : IMappingEngine, IMappingEngineRunner
	{
	    private bool _disposed;
		private readonly IConfigurationProvider _configurationProvider;
		private readonly IObjectMapper[] _mappers;
        private readonly ConcurrentDictionary<TypePair, IObjectMapper> _objectMapperCache = new ConcurrentDictionary<TypePair, IObjectMapper>();
        private readonly ConcurrentDictionary<TypePair, LambdaExpression> _expressionCache = new ConcurrentDictionary<TypePair, LambdaExpression>();

		public MappingEngine(IConfigurationProvider configurationProvider)
		{
			_configurationProvider = configurationProvider;
			_mappers = configurationProvider.GetMappers();
            _configurationProvider.TypeMapCreated += ClearTypeMap;
		}

		public IConfigurationProvider ConfigurationProvider
		{
			get { return _configurationProvider; }
		}

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
                    if (_configurationProvider != null)
                        _configurationProvider.TypeMapCreated -= ClearTypeMap;
                }

                _disposed = true;
            }
        }

	    public TDestination Map<TDestination>(object source)
        {
            return Map<TDestination>(source, opts => { });
        }

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            var mappedObject = default(TDestination);
            if (source != null)
            {
                var sourceType = source.GetType();
                var destinationType = typeof(TDestination);

                mappedObject = (TDestination)Map(source, sourceType, destinationType, opts);
            }
            return mappedObject;
        }

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)Map(source, modelType, destinationType, opts => {});
		}

        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions> opts)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            return (TDestination) Map(source, modelType, destinationType, opts);
        }

	    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
		    return Map(source, destination, opts => { });
		}

	    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions> opts)
	    {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            return (TDestination)Map(source, destination, modelType, destinationType, opts);
        }

	    public object Map(object source, Type sourceType, Type destinationType)
	    {
	        return Map(source, sourceType, destinationType, opt => { });
	    }

	    public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
	    {
            TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);

	        var options = new MappingOperationOptions();

            opts(options);

	        var context = new ResolutionContext(typeMap, source, sourceType, destinationType, options);

            return ((IMappingEngineRunner)this).Map(context);
	    }

	    public object Map(object source, object destination, Type sourceType, Type destinationType)
	    {
	        return Map(source, destination, sourceType, destinationType, opts => { });
        }

	    public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
	    {
            TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);

	        var options = new MappingOperationOptions();

            opts(options);

	        var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, options);

            return ((IMappingEngineRunner)this).Map(context);
        }


	    public TDestination DynamicMap<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)DynamicMap(source, modelType, destinationType);
		}

		public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			DynamicMap(source, destination, modelType, destinationType);
		}

		public TDestination DynamicMap<TDestination>(object source)
		{
			Type modelType = source == null ? typeof(object) : source.GetType();
			Type destinationType = typeof(TDestination);

			return (TDestination)DynamicMap(source, modelType, destinationType);
		}

		public object DynamicMap(object source, Type sourceType, Type destinationType)
		{
			var typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType) ??
			              ConfigurationProvider.CreateTypeMap(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType, new MappingOperationOptions());

			return ((IMappingEngineRunner)this).Map(context);
		}

		public void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
		{
			var typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType) ??
			              ConfigurationProvider.CreateTypeMap(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, new MappingOperationOptions());

			((IMappingEngineRunner)this).Map(context);
		}

        public Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>()
        {
            return (Expression<Func<TSource, TDestination>>) 
                _expressionCache.GetOrAdd(new TypePair(typeof (TSource), typeof (TDestination)), tp =>
            {
                return CreateMapExpression(tp.SourceType, tp.DestinationType);
            });
        }

        public TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source)
        {
            Type destinationType = typeof(TDestination);
            Type sourceType = typeof(TSource);
            TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);
            var context = parentContext.CreateTypeContext(typeMap, source, sourceType, destinationType);
            return (TDestination)((IMappingEngineRunner)this).Map(context);
        }

        private LambdaExpression CreateMapExpression(
            Type typeIn, Type typeOut)
        {
            var typeMap = ConfigurationProvider.FindTypeMapFor(typeIn, typeOut);

            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(typeIn);

            var bindings = new List<MemberBinding>();
            foreach (var propertyMap in typeMap.GetPropertyMaps())
            {
                var destinationProperty = propertyMap.DestinationProperty;
                var destinationMember = destinationProperty.MemberInfo;

                Expression currentChild = instanceParameter;
                Type currentChildType = typeIn;
                foreach (var resolver in propertyMap.GetSourceValueResolvers())
                {
                    var getter = resolver as IMemberGetter;
                    if (getter != null)
                    {
                        var memberInfo = getter.MemberInfo;

                        var propertyInfo = memberInfo as PropertyInfo;
                        if (propertyInfo != null)
                        {
                            currentChild = Expression.Property(currentChild, propertyInfo);
                            currentChildType = propertyInfo.PropertyType;
                        }
                    }
                    else
                    {
                        var oldParameter =
                            ((LambdaExpression)propertyMap.CustomExpression).Parameters.Single();
                        var newParameter = instanceParameter;
                        var converter = new ConversionVisitor(newParameter, oldParameter);
                        currentChild = converter.Visit(((LambdaExpression)propertyMap.CustomExpression).Body);
                        var propertyInfo = propertyMap.SourceMember as PropertyInfo;
                        if (propertyInfo != null)
                        {
                            currentChildType = propertyInfo.PropertyType;
                        }
                    }
                }

                var prop = destinationProperty.MemberInfo as PropertyInfo;

                // next to lists, also arrays
                // and objects!!!
                if (prop != null &&
                    prop.PropertyType.GetInterface("IEnumerable", true) != null &&
                    prop.PropertyType != typeof(string))
                {

                    Type destinationListType = prop.PropertyType.GetGenericArguments().First();
                    Type sourceListType = null;
                    // is list

                    sourceListType = currentChildType.GetGenericArguments().First();

                    //var newVariableName = "t" + (i++);
                    var transformedExpression = CreateMapExpression(sourceListType, destinationListType);

                    MethodCallExpression selectExpression = Expression.Call(
                                typeof(Enumerable),
                                "Select",
                                new[] { sourceListType, destinationListType },
                                currentChild,
                                transformedExpression);
                    MethodCallExpression toListCallExpression = Expression.Call(
                        typeof(Enumerable),
                        "ToList",
                        new Type[] { destinationListType },
                        selectExpression);

                    // todo .ToArray()
                    bindings.Add(Expression.Bind(destinationMember, toListCallExpression));
                }
                else
                {
                    // does of course not work for subclasses etc./generic ...
                    if (currentChildType != prop.PropertyType &&
                        // avoid nullable etc.
                        prop.PropertyType.BaseType != typeof(ValueType) && 
                        prop.PropertyType.BaseType != typeof(Enum))
                    {
                        var transformedExpression = CreateMapExpression(currentChildType, prop.PropertyType);
                        var expr2 = Expression.Invoke(
                            transformedExpression,
                            currentChild
                        );
                        bindings.Add(Expression.Bind(destinationMember, expr2));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(destinationMember, currentChild));
                    }
                }

            }
            var total = Expression.MemberInit(
                Expression.New(typeOut),
                bindings.ToArray()
            );

            return Expression.Lambda(total, instanceParameter);
        }


		object IMappingEngineRunner.Map(ResolutionContext context)
		{
			try
			{
				var contextTypePair = new TypePair(context.SourceType, context.DestinationType);

			    Func<TypePair, IObjectMapper> missFunc = tp => _mappers.FirstOrDefault(mapper => mapper.IsMatch(context));

			    IObjectMapper mapperToUse = _objectMapperCache.GetOrAdd(contextTypePair, missFunc);

				if (mapperToUse == null)
				{
                    if (context.SourceValue != null)
					    throw new AutoMapperMappingException(context, "Missing type map configuration or unsupported mapping.");

				    return ObjectCreator.CreateDefaultValue(context.DestinationType);
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

		string IMappingEngineRunner.FormatValue(ResolutionContext context)
		{
			TypeMap contextTypeMap = context.GetContextTypeMap();
			IFormatterConfiguration configuration = contextTypeMap != null
												? ConfigurationProvider.GetProfileConfiguration(contextTypeMap.Profile)
                                                : ConfigurationProvider.GetProfileConfiguration(ConfigurationStore.DefaultProfileName);

            object valueToFormat = context.SourceValue;
            string formattedValue = context.SourceValue.ToNullSafeString();

            var formatters = configuration.GetFormattersToApply(context);

            foreach (var valueFormatter in formatters)
            {
                formattedValue = valueFormatter.FormatValue(context.CreateValueContext(valueToFormat));

                valueToFormat = formattedValue;
            }

            if (formattedValue == null && !((IMappingEngineRunner)this).ShouldMapSourceValueAsNull(context))
                return string.Empty;

		    return formattedValue;
		}

		object IMappingEngineRunner.CreateObject(ResolutionContext context)
		{
			var typeMap = context.TypeMap;
            var destinationType = context.DestinationType;

            if (typeMap != null)
                if (typeMap.DestinationCtor != null)
                    return typeMap.DestinationCtor(context);
                else if (typeMap.ConstructDestinationUsingServiceLocator && context.Options.ServiceCtor != null)
                    return context.Options.ServiceCtor(destinationType);
                else if (typeMap.ConstructDestinationUsingServiceLocator)
                    return _configurationProvider.ServiceCtor(destinationType);
                else if (typeMap.ConstructorMap != null)
                    return typeMap.ConstructorMap.ResolveValue(context, this);

			if (context.DestinationValue != null)
				return context.DestinationValue;

            if (destinationType.IsInterface)
                destinationType = ProxyGenerator.GetProxyType(destinationType);

			return ObjectCreator.CreateObject(destinationType);
		}

        bool IMappingEngineRunner.ShouldMapSourceValueAsNull(ResolutionContext context)
		{
            if (context.DestinationType.IsValueType)
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

        /// <summary>
        /// This expression visitor will replace an input parameter by another one
        /// 
        /// see http://stackoverflow.com/questions/4601844/expression-tree-copy-or-convert
        /// </summary>
        private class ConversionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression newParameter;
            private readonly ParameterExpression oldParameter;

            public ConversionVisitor(ParameterExpression newParameter, ParameterExpression oldParameter)
            {
                this.newParameter = newParameter;
                this.oldParameter = oldParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // replace all old param references with new ones
                return node.Type == oldParameter.Type ? newParameter : node; 
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != oldParameter) // if instance is not old parameter - do nothing
                    return base.VisitMember(node);

                var newObj = Visit(node.Expression);
                var newMember = newParameter.Type.GetMember(node.Member.Name).First();
                return Expression.MakeMemberAccess(newObj, newMember);
            }
        }

	}
}
