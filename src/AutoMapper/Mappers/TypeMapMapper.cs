using System;

namespace AutoMapper.Mappers
{
	public class TypeMapMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			var profileConfiguration = mapper.Configuration.GetProfileConfiguration(context.SourceValueTypeMap.Profile);

			if (context.SourceValue == null && profileConfiguration.MapNullSourceValuesAsNull)
			{
				return null;
			}

			object mappedObject = context.DestinationValue ?? mapper.CreateObject(context.DestinationType);

			foreach (PropertyMap propertyMap in context.SourceValueTypeMap.GetPropertyMaps())
			{
				if (!propertyMap.CanResolveValue())
				{
					continue;
				}

				var result = propertyMap.ResolveValue(context.SourceValue);

				var memberTypeMap = mapper.Configuration.FindTypeMapFor(result.Type, propertyMap.DestinationProperty.MemberType);

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
			return context.SourceValueTypeMap != null;
		}
	}
}