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
			    MapPropertyValue(context, mapper, mappedObject, propertyMap);
			}

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
	                ? typeof (object)
	                : context.SourceValue.GetType(),
	            propertyMap);
	    }
	}
}