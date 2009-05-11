using System;

namespace AutoMapper.Mappers
{
	public class TypeMapMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			var profileConfiguration = mapper.ConfigurationProvider.GetProfileConfiguration(context.TypeMap.Profile);

			if (context.SourceValue == null && profileConfiguration.MapNullSourceValuesAsNull)
			{
				return null;
			}

			object mappedObject = context.DestinationValue;

			if (mappedObject == null)
			{
				if (context.SourceValue != null && context.InstanceCache.ContainsKey(context.SourceValue))
					return context.InstanceCache[context.SourceValue];

				mappedObject = mapper.CreateObject(context.DestinationType);

				if (context.SourceValue != null)
					context.InstanceCache.Add(context.SourceValue, mappedObject);
			}

			foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
			{
				if (!propertyMap.CanResolveValue())
				{
					continue;
				}

				var result = propertyMap.ResolveValue(context.SourceValue);

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