using System;

namespace AutoMapper.Mappers
{
	public class TypeMapMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			if (context.TypeMap.CustomMapper != null)
			{
				return context.TypeMap.CustomMapper(context);				
			}

			var profileConfiguration = mapper.ConfigurationProvider.GetProfileConfiguration(context.TypeMap.Profile);

			if (context.SourceValue == null && profileConfiguration.MapNullSourceValuesAsNull)
			{
				return null;
			}

			object mappedObject = context.DestinationValue;

			if (mappedObject == null)
			{
				if (context.InstanceCache.ContainsKey(context))
					return context.InstanceCache[context];

				mappedObject = mapper.CreateObject(context.DestinationType);

				if (context.SourceValue != null)
					context.InstanceCache.Add(context, mappedObject);
			}

			foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
			{
				if (!propertyMap.CanResolveValue())
				{
					continue;
				}

				ResolutionResult result;

				try
				{
					result = propertyMap.ResolveValue(context.SourceValue);
				}
				catch (Exception ex)
				{
					var errorContext = context.CreateMemberContext(null, context.SourceValue, context.SourceValue == null
					                                                                          	? typeof (object)
					                                                                          	: context.SourceValue.GetType(), propertyMap);
					throw new AutoMapperMappingException(errorContext, ex);
				}

				var memberTypeMap = mapper.ConfigurationProvider.FindTypeMapFor(result.Type, propertyMap.DestinationProperty.MemberType);

				var newContext = context.CreateMemberContext(memberTypeMap, result.Value, result.Type, propertyMap);

				try
				{
					object propertyValueToAssign = mapper.Map(newContext);
					propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
				}
				catch (Exception ex)
				{
					throw new AutoMapperMappingException(newContext, ex);
				}

			}

			return mappedObject;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.TypeMap != null;
		}
	}
}