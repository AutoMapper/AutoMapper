using System;

namespace AutoMapper.Mappers
{
	public interface ITypeMapObjectMapper
	{
		object Map(ResolutionContext context, IMappingEngineRunner mapper);
		bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper);
	}

	public class TypeMapMappingStrategyFactory
	{
		public ITypeMapObjectMapper[] CreateStrategies()
		{
			return new[]
			{
				new CustomMapperStrategy(),
				//new 
			};
		}

		private class CustomMapperStrategy : ITypeMapObjectMapper
		{
			public object Map(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return context.TypeMap.CustomMapper(context);
			}

			public bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return context.TypeMap.CustomMapper != null;
			}
		}

		private class NullMappingStrategy : ITypeMapObjectMapper
		{
			public object Map(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return null;
			}

			public bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper)
			{
				var profileConfiguration = mapper.ConfigurationProvider.GetProfileConfiguration(context.TypeMap.Profile);
				return (context.SourceValue == null && profileConfiguration.MapNullSourceValuesAsNull);
			}
		}
	}

	public class TypeMapMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			object mappedObject = null;
			var profileConfiguration = mapper.ConfigurationProvider.GetProfileConfiguration(context.TypeMap.Profile);

			if (context.TypeMap.CustomMapper != null)
			{
				context.TypeMap.BeforeMap(context.SourceValue, mappedObject);
				mappedObject = context.TypeMap.CustomMapper(context);
			}
			else if ((context.SourceValue == null && profileConfiguration.MapNullSourceValuesAsNull))
			{
				mappedObject = null;
			}
			else if (context.DestinationValue == null && context.InstanceCache.ContainsKey(context))
			{
				mappedObject = context.InstanceCache[context];
			}
			else if (context.DestinationValue == null)
			{
				mappedObject = mapper.CreateObject(context.DestinationType);
				if (context.SourceValue != null)
					context.InstanceCache.Add(context, mappedObject);

				context.TypeMap.BeforeMap(context.SourceValue, mappedObject);
				foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
				{
					MapPropertyValue(context, mapper, mappedObject, propertyMap);
				}
			}
			else
			{
				mappedObject = context.DestinationValue;

				context.TypeMap.BeforeMap(context.SourceValue, mappedObject);
				foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
				{
					MapPropertyValue(context, mapper, mappedObject, propertyMap);
				}
			}


			context.TypeMap.AfterMap(context.SourceValue, mappedObject);
			return mappedObject;
		}


		public bool IsMatch(ResolutionContext context)
		{
			return context.TypeMap != null;
		}

		private void MapPropertyValue(ResolutionContext context, IMappingEngineRunner mapper, object mappedObject, PropertyMap propertyMap)
		{
			if (propertyMap.CanResolveValue())
			{
				object destinationValue = null;
				ResolutionResult result;

				try
				{
					result = propertyMap.ResolveValue(context.SourceValue);
				}
				catch (Exception ex)
				{
					var errorContext = CreateErrorContext(context, propertyMap, destinationValue);
					throw new AutoMapperMappingException(errorContext, ex);
				}

				if (propertyMap.UseDestinationValue)
				{
					destinationValue = propertyMap.DestinationProperty.GetValue(mappedObject);
				}

				var sourceType = result.Type;
				var destinationType = propertyMap.DestinationProperty.MemberType;

				var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(result.Value, sourceType, destinationType);

				Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceType;

				var newContext = context.CreateMemberContext(typeMap, result.Value, destinationValue, targetSourceType,
															 propertyMap);

				try
				{
					object propertyValueToAssign = mapper.Map(newContext);

					if (!propertyMap.UseDestinationValue)
						propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
				}
				catch (Exception ex)
				{
					throw new AutoMapperMappingException(newContext, ex);
				}
			}
		}

		private ResolutionContext CreateErrorContext(ResolutionContext context, PropertyMap propertyMap, object destinationValue)
		{
			return context.CreateMemberContext(
				null,
				context.SourceValue,
				destinationValue,
				context.SourceValue == null
					? typeof(object)
					: context.SourceValue.GetType(),
				propertyMap);
		}
	}
}