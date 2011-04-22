using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper.Internal;
using AutoMapper.Mappers;
using Castle.DynamicProxy;

namespace AutoMapper
{
	public class MappingEngine : IMappingEngine, IMappingEngineRunner
	{
		private readonly IConfigurationProvider _configurationProvider;
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
		private readonly IObjectMapper[] _mappers;
        private readonly ConcurrentDictionary<TypePair, IObjectMapper> _objectMapperCache = new ConcurrentDictionary<TypePair, IObjectMapper>();

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

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)Map(source, modelType, destinationType);
		}

		public TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source)
		{
			Type destinationType = typeof(TDestination);
			Type sourceType = typeof(TSource);
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);
			var context = parentContext.CreateTypeContext(typeMap, source, sourceType, destinationType);
			return (TDestination)((IMappingEngineRunner)this).Map(context);
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)Map(source, destination, modelType, destinationType);
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

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType);

			return ((IMappingEngineRunner)this).Map(context);
		}

		public void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
		{
			var typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType) ??
			              ConfigurationProvider.CreateTypeMap(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType);

			((IMappingEngineRunner)this).Map(context);
		}

		public object Map(object source, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType);

			return ((IMappingEngineRunner)this).Map(context);
		}

		public object Map(object source, object destination, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType);

			return ((IMappingEngineRunner)this).Map(context);
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

			if (typeMap != null && typeMap.DestinationCtor != null)
				return typeMap.DestinationCtor(context.SourceValue);

			if (context.DestinationValue != null)
				return context.DestinationValue;

			var destinationType = context.DestinationType;

            if (destinationType.IsInterface)
            {
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(destinationType))
                    return _proxyGenerator.CreateInterfaceProxyWithoutTarget(destinationType, new[] { typeof(INotifyPropertyChanged) }, new NotifyPropertyBehaviorInterceptor());
                return _proxyGenerator.CreateInterfaceProxyWithoutTarget(destinationType, new PropertyBehaviorInterceptor());
            }

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

		private void ClearTypeMap(object sender, TypeMapCreatedEventArgs e)
		{
		    IObjectMapper existing;

		    _objectMapperCache.TryRemove(new TypePair(e.TypeMap.SourceType, e.TypeMap.DestinationType), out existing);
		}
	}
}
