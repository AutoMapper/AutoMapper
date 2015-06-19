namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public static class TypeMapObjectMapperRegistry
    {
        /// <summary>
        /// Extension point for mappers matching based on types configured by CreateMap.
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

        /// <summary>
        /// 
        /// </summary>
        private class CustomMapperStrategy : ITypeMapObjectMapper
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public object Map(ResolutionContext context)
            {
                return context.TypeMap.CustomMapper(context);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool IsMatch(ResolutionContext context)
            {
                return context.TypeMap.CustomMapper != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class SubstutitionMapperStrategy : ITypeMapObjectMapper
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public object Map(ResolutionContext context)
            {
                var newSource = context.TypeMap.Substitution(context.SourceValue);
                var typeMap = context.MapperContext.ConfigurationProvider.ResolveTypeMap(newSource.GetType(), context.DestinationType);

                var substitutionContext = context.CreateTypeContext(typeMap, newSource, context.DestinationValue,
                    newSource.GetType(), context.DestinationType);

                return context.MapperContext.Runner.Map(substitutionContext);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool IsMatch(ResolutionContext context)
            {
                return context.TypeMap.Substitution != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class NullMappingStrategy : ITypeMapObjectMapper
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public object Map(ResolutionContext context)
            {
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool IsMatch(ResolutionContext context)
            {
                var profileConfiguration = context.MapperContext.ConfigurationProvider.GetProfileConfiguration(context.TypeMap.Profile);
                return (context.SourceValue == null && profileConfiguration.MapNullSourceValuesAsNull);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class CacheMappingStrategy : ITypeMapObjectMapper
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public object Map(ResolutionContext context)
            {
                return context.InstanceCache[context];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool IsMatch(ResolutionContext context)
            {
                return !context.Options.DisableCache
                       && context.DestinationValue == null
                       && context.InstanceCache.ContainsKey(context);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private abstract class PropertyMapMappingStrategy : ITypeMapObjectMapper
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public object Map(ResolutionContext context)
            {
                var mappedObject = GetMappedObject(context);
                if (context.SourceValue != null && !context.Options.DisableCache)
                    context.InstanceCache[context] = mappedObject;

                context.TypeMap.BeforeMap(context.SourceValue, mappedObject);
                context.Options.BeforeMapAction(context.SourceValue, mappedObject);

                foreach (var propertyMap in context.TypeMap.GetPropertyMaps())
                {
                    MapPropertyValue(context.CreatePropertyMapContext(propertyMap), mappedObject, propertyMap);
                }
                mappedObject = ReassignValue(context, mappedObject);

                context.Options.AfterMapAction(context.SourceValue, mappedObject);
                context.TypeMap.AfterMap(context.SourceValue, mappedObject);

                return mappedObject;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <param name="o"></param>
            /// <returns></returns>
            protected virtual object ReassignValue(ResolutionContext context, object o)
            {
                return o;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public abstract bool IsMatch(ResolutionContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            protected abstract object GetMappedObject(ResolutionContext context);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <param name="mappedObject"></param>
            /// <param name="propertyMap"></param>
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

                var destinationValue = propertyMap.GetDestinationValue(mappedObject);

                var sourceType = result.Type;
                var destinationType = propertyMap.DestinationProperty.MemberType;

                var typeMap = context.MapperContext.ConfigurationProvider.ResolveTypeMap(result, destinationType);

                var targetSourceType = typeMap != null ? typeMap.SourceType : sourceType;

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
                    var propertyValueToAssign = context.MapperContext.Runner.Map(newContext);

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

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyMap"></param>
            /// <param name="mappedObject"></param>
            /// <param name="propertyValueToAssign"></param>
            protected virtual void AssignValue(PropertyMap propertyMap, object mappedObject,
                object propertyValueToAssign)
            {
                if (propertyMap.CanBeSet)
                    propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <param name="propertyMap"></param>
            /// <param name="destinationValue"></param>
            /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        private class NewObjectPropertyMapMappingStrategy : PropertyMapMappingStrategy
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public override bool IsMatch(ResolutionContext context)
            {
                return context.DestinationValue == null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            protected override object GetMappedObject(ResolutionContext context)
            {
                var result = context.MapperContext.Runner.CreateObject(context);
                context.SetResolvedDestinationValue(result);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class ExistingObjectMappingStrategy : PropertyMapMappingStrategy
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public override bool IsMatch(ResolutionContext context)
            {
                return true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            protected override object GetMappedObject(ResolutionContext context)
            {
                var result = context.DestinationValue;
                context.SetResolvedDestinationValue(result);
                return result;
            }
        }
    }
}