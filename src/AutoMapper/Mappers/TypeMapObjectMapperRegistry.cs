namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;

    public static class TypeMapObjectMapperRegistry
    {
        /// <summary>
        /// Extension point for mappers matching based on types configured by CreateMap
        /// </summary>
        public static IList<ITypeMapObjectMapper> Mappers { get; } = new List<ITypeMapObjectMapper>
        {
            new PropertyMapMappingStrategy()
        };

        private class PropertyMapMappingStrategy : ITypeMapObjectMapper
        {
            public object Map(object source, ResolutionContext context)
            {
                if (context.TypeMap.Substitution != null)
                {
                    var newSource = context.TypeMap.Substitution(context.SourceValue);

                    return context.Mapper.Map(newSource, context.DestinationValue, newSource.GetType(), context.DestinationType, context);
                }

                if (context.TypeMap.CustomMapper != null)
                {
                    return context.TypeMap.CustomMapper(source, context);
                }

                if (context.SourceValue == null && context.TypeMap.Profile.AllowNullDestinationValues)
                {
                    return null;
                }

                if (!context.Options.DisableCache && context.DestinationValue == null &&
                    context.InstanceCache.ContainsKey(context))
                {
                    return context.InstanceCache[context];
                }

                var mappedObject = context.DestinationValue ?? context.Mapper.CreateObject(context);

                if (mappedObject == null)
                {
                    throw new InvalidOperationException("Cannot create destination object. " + context);
                }

                if (context.SourceValue != null && !context.Options.DisableCache)
                    context.InstanceCache[context] = mappedObject;

                context.TypeMap.BeforeMap(context.SourceValue, mappedObject, context);
                context.BeforeMap(mappedObject);

                ResolutionContext propertyContext = new ResolutionContext(context);
                foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
                {
                    try
                    {
                        MapPropertyValue(context, propertyContext, mappedObject, propertyMap);
                    }
                    catch (AutoMapperMappingException ex)
                    {
                        ex.PropertyMap = propertyMap;

                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new AutoMapperMappingException(context, ex) { PropertyMap = propertyMap };
                    }
                }

                context.AfterMap(mappedObject);
                context.TypeMap.AfterMap(context.SourceValue, mappedObject, context);

                return mappedObject;
            }

            public bool IsMatch(ResolutionContext context)
            {
                return true;
            }

            private void MapPropertyValue(ResolutionContext context, ResolutionContext propertyContext, object mappedObject, PropertyMap propertyMap)
            {
                if (!propertyMap.CanResolveValue() || !propertyMap.ShouldAssignValuePreResolving(context))
                    return;

                var result = propertyMap.ResolveValue(context);

                object destinationValue = propertyMap.GetDestinationValue(mappedObject);

                var declaredSourceType = propertyMap.SourceType ?? context.SourceType;
                var sourceType = result?.GetType() ?? declaredSourceType;
                var destinationType = propertyMap.DestinationProperty.MemberType;

                if (!propertyMap.ShouldAssignValue(result, destinationValue, context))
                    return;

                var typeMap = context.ConfigurationProvider.ResolveTypeMap(result, destinationValue, sourceType, destinationType);
                propertyContext.Fill(result, destinationValue, sourceType, destinationType, typeMap);

                object propertyValueToAssign = context.Mapper.Map(propertyContext);

                if (propertyMap.CanBeSet)
                    propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
            }
        }
    }
}