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

			    object destinationValue = null;
				ResolutionResult result;

				try
				{
					result = propertyMap.ResolveValue(context.SourceValue);
				}
				catch (Exception ex)
				{
				    var errorContext =
				        context.CreateMemberContext(
				            null,
				            context.SourceValue,
				            destinationValue,
				            context.SourceValue == null
				                ? typeof (object)
				                : context.SourceValue.GetType(),
				            propertyMap);
					throw new AutoMapperMappingException(errorContext, ex);
				}

                if (propertyMap.UseDestinationValue)
                {
                    destinationValue = propertyMap.DestinationProperty.GetValue(mappedObject);
                }

                // Should refactor this back out to FindTypeMapFor or something like that
                Type targetSourceType = result.Type;
                Type targetDestinationType = propertyMap.DestinationProperty.MemberType;

                if (result.Type != result.MemberType)
                {
                    var potentialSourceType = targetSourceType;

                    TypeMap itemTypeMap =
                        mapper.ConfigurationProvider.FindTypeMapFor(result.Value, result.MemberType, targetDestinationType)
                        ?? mapper.ConfigurationProvider.FindTypeMapFor(result.Value, potentialSourceType, targetDestinationType);

                    if (itemTypeMap != null)
                    {
                        var potentialDestType = itemTypeMap.GetDerivedTypeFor(potentialSourceType);

                        targetSourceType = potentialDestType != targetDestinationType
                                            ? potentialSourceType
                                            : itemTypeMap.SourceType;
                        targetDestinationType = potentialDestType;
                    }
                }

                TypeMap memberTypeMap = mapper.ConfigurationProvider.FindTypeMapFor(result.Value, targetSourceType, targetDestinationType);

				var newContext = context.CreateMemberContext(memberTypeMap, result.Value, destinationValue, targetSourceType, propertyMap);

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

			return mappedObject;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.TypeMap != null;
		}
	}
}