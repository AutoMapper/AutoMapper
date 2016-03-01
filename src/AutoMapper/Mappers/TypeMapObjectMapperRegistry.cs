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
            new SubstutitionMapperStrategy(),
            new CustomMapperStrategy(),
            new NullMappingStrategy(),
            new CacheMappingStrategy(),
            new NewObjectPropertyMapMappingStrategy(),
            new ExistingObjectMappingStrategy()
        };

        private class CustomMapperStrategy : ITypeMapObjectMapper
        {
            public object Map(object source, ResolutionContext context)
            {
                return context.TypeMap.CustomMapper(source, context);
            }

            public bool IsMatch(ResolutionContext context)
            {
                return context.TypeMap.CustomMapper != null;
            }
        }

        private class SubstutitionMapperStrategy : ITypeMapObjectMapper
        {
            public object Map(object source, ResolutionContext context)
            {
                var newSource = context.TypeMap.Substitution(context.SourceValue);

                return context.Mapper.Map(newSource, context.DestinationValue, newSource.GetType(), context.DestinationType, context);
            }

            public bool IsMatch(ResolutionContext context)
            {
                return context.TypeMap.Substitution != null;
            }
        }

        private class NullMappingStrategy : ITypeMapObjectMapper
        {
            public object Map(object source, ResolutionContext context)
            {
                return null;
            }

            public bool IsMatch(ResolutionContext context)
            {
                return context.SourceValue == null && context.TypeMap.Profile.AllowNullDestinationValues;
            }
        }

        private class CacheMappingStrategy : ITypeMapObjectMapper
        {
            public object Map(object source, ResolutionContext context)
            {
                return context.InstanceCache[context];
            }

            public bool IsMatch(ResolutionContext context)
            {
                return !context.Options.DisableCache && context.DestinationValue == null &&
                       context.InstanceCache.ContainsKey(context);
            }
        }

        private abstract class PropertyMapMappingStrategy : ITypeMapObjectMapper
        {
            public object Map(object source, ResolutionContext context)
            {
                var mappedObject = GetMappedObject(context);
                if (context.SourceValue != null && !context.Options.DisableCache)
                    context.InstanceCache[context] = mappedObject;

                context.TypeMap.BeforeMap(context.SourceValue, mappedObject, context);
                context.BeforeMap(mappedObject);

                var propertyContext = new ResolutionContext(context);
                foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
                {
                    MapPropertyValue(context, propertyContext, mappedObject, propertyMap);
                }
                mappedObject = ReassignValue(context, mappedObject);

                context.AfterMap(mappedObject);
                context.TypeMap.AfterMap(context.SourceValue, mappedObject, context);

                return mappedObject;
            }

            protected virtual object ReassignValue(ResolutionContext context, object o)
            {
                return o;
            }

            public abstract bool IsMatch(ResolutionContext context);

            protected abstract object GetMappedObject(ResolutionContext context);

            private void MapPropertyValue(ResolutionContext parentContext, ResolutionContext propertyContext, object mappedObject, PropertyMap propertyMap)
            {
                if (!propertyMap.CanResolveValue() || !propertyMap.ShouldAssignValuePreResolving(parentContext))
                    return;

                object result;
                Exception resolvingExc = null;
                try
                {
                    result = propertyMap.ResolveValue(parentContext);
                }
                catch (AutoMapperMappingException ex)
                {
                    ex.PropertyMap = propertyMap;

                    throw;
                }
                catch (Exception ex)
                {
                    resolvingExc = new AutoMapperMappingException(parentContext, ex) { PropertyMap = propertyMap };
                    result = parentContext.SourceValue;
                }

                object destinationValue = propertyMap.GetDestinationValue(mappedObject);

                var declaredSourceType = propertyMap.SourceType ?? parentContext.SourceType;
                var sourceType = result?.GetType() ?? declaredSourceType;
                var destinationType = propertyMap.DestinationProperty.MemberType;

                var typeMap = parentContext.ConfigurationProvider.ResolveTypeMap(result, destinationValue, sourceType, destinationType);
                propertyContext.Fill(result, destinationValue, sourceType, destinationType, typeMap);

                if (!propertyMap.ShouldAssignValue(propertyContext))
                    return;

                // If condition succeeded and resolving failed, throw
                if (resolvingExc != null)
                    throw resolvingExc;

                try
                {
                    object propertyValueToAssign = parentContext.Mapper.Map(propertyContext);

                    AssignValue(propertyMap, mappedObject, propertyValueToAssign);
                }
                catch (AutoMapperMappingException ex)
                {
                    ex.PropertyMap = propertyMap;

                    throw;
                }
                catch (Exception ex)
                {
                    throw new AutoMapperMappingException(propertyContext, ex);
                }
            }

            protected virtual void AssignValue(PropertyMap propertyMap, object mappedObject,
                object propertyValueToAssign)
            {
                if (propertyMap.CanBeSet)
                    propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
            }
        }

        private class NewObjectPropertyMapMappingStrategy : PropertyMapMappingStrategy
        {
            public override bool IsMatch(ResolutionContext context)
            {
                return context.DestinationValue == null;
            }

            protected override object GetMappedObject(ResolutionContext context)
            {
                var result = context.Mapper.CreateObject(context);
                if(result == null)
                {
                    throw new InvalidOperationException("Cannot create destination object. " + context);
                }
                return result;
            }
        }

        private class ExistingObjectMappingStrategy : PropertyMapMappingStrategy
        {
            public override bool IsMatch(ResolutionContext context)
            {
                return true;
            }

            protected override object GetMappedObject(ResolutionContext context)
            {
                return context.DestinationValue;
            }
        }
    }
}