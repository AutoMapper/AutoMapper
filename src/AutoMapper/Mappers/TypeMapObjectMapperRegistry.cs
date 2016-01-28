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
            public object Map(ResolutionContext context)
            {
                return context.TypeMap.CustomMapper(context);
            }

            public bool IsMatch(ResolutionContext context)
            {
                return context.TypeMap.CustomMapper != null;
            }
        }

        private class SubstutitionMapperStrategy : ITypeMapObjectMapper
        {
            public object Map(ResolutionContext context)
            {
                var newSource = context.TypeMap.Substitution(context.SourceValue);
                var typeMap = context.ConfigurationProvider.ResolveTypeMap(newSource.GetType(), context.DestinationType);

                var substitutionContext = context.CreateTypeContext(typeMap, newSource, context.DestinationValue,
                    newSource.GetType(), context.DestinationType);

                return context.Engine.Map(substitutionContext);
            }

            public bool IsMatch(ResolutionContext context)
            {
                return context.TypeMap.Substitution != null;
            }
        }

        private class NullMappingStrategy : ITypeMapObjectMapper
        {
            public object Map(ResolutionContext context)
            {
                return null;
            }

            public bool IsMatch(ResolutionContext context)
            {
                var profileConfiguration = context.ConfigurationProvider.GetProfileConfiguration(context.TypeMap.Profile);
                return (context.SourceValue == null && profileConfiguration.AllowNullDestinationValues);
            }
        }

        private class CacheMappingStrategy : ITypeMapObjectMapper
        {
            public object Map(ResolutionContext context)
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
            public object Map(ResolutionContext context)
            {
                var mappedObject = GetMappedObject(context);
                if (context.SourceValue != null && !context.Options.DisableCache)
                    context.InstanceCache[context] = mappedObject;

                context.TypeMap.BeforeMap(context.SourceValue, mappedObject);
                context.BeforeMap(mappedObject);

                foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
                {
                    MapPropertyValue(context.CreatePropertyMapContext(propertyMap), mappedObject, propertyMap);
                }
                mappedObject = ReassignValue(context, mappedObject);

                context.AfterMap(mappedObject);
                context.TypeMap.AfterMap(context.SourceValue, mappedObject);

                return mappedObject;
            }

            protected virtual object ReassignValue(ResolutionContext context, object o)
            {
                return o;
            }

            public abstract bool IsMatch(ResolutionContext context);

            protected abstract object GetMappedObject(ResolutionContext context);

            private void MapPropertyValue(ResolutionContext context, object mappedObject, PropertyMap propertyMap)
            {
                if (!propertyMap.CanResolveValue() || !propertyMap.ShouldAssignValuePreResolving(context))
                    return;

                ResolutionResult result;

                Exception resolvingExc = null;
                try
                {
                    result = propertyMap.ResolveValue(context);
                }
                catch (AutoMapperMappingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var errorContext = CreateErrorContext(context, propertyMap, null);
                    resolvingExc = new AutoMapperMappingException(errorContext, ex);

                    result = new ResolutionResult(context);
                }

                if (result.ShouldIgnore)
                    return;

                object destinationValue = propertyMap.GetDestinationValue(mappedObject);

                var sourceType = result.Type;
                var destinationType = propertyMap.DestinationProperty.MemberType;

                var typeMap = context.ConfigurationProvider.ResolveTypeMap(result, destinationType);

                Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceType;

                var newContext = context.CreateMemberContext(typeMap, result.Value, destinationValue,
                    targetSourceType,
                    propertyMap);

                if (!propertyMap.ShouldAssignValue(newContext))
                    return;

                // If condition succeeded and resolving failed, throw
                if (resolvingExc != null)
                    throw resolvingExc;

                try
                {
                    object propertyValueToAssign = context.Engine.Map(newContext);

                    AssignValue(propertyMap, mappedObject, propertyValueToAssign);
                }
                catch (AutoMapperMappingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AutoMapperMappingException(newContext, ex);
                }
            }

            protected virtual void AssignValue(PropertyMap propertyMap, object mappedObject,
                object propertyValueToAssign)
            {
                if (propertyMap.CanBeSet)
                    propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
            }

            private ResolutionContext CreateErrorContext(ResolutionContext context, PropertyMap propertyMap,
                object destinationValue)
            {
                return context.CreateMemberContext(
                    null,
                    context.SourceValue,
                    destinationValue,
                    context.SourceValue?.GetType() ?? typeof (object),
                    propertyMap);
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
                var result = context.Engine.CreateObject(context);
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