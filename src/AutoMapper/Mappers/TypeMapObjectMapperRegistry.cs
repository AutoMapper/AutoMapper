using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
	public class TypeMapObjectMapperRegistry
	{
        /// <summary>
        /// Extension point for mappers matching based on types configured by CreateMap
        /// </summary>
		public static Func<IEnumerable<ITypeMapObjectMapper>> AllMappers =
			() => new ITypeMapObjectMapper[]
            {
                new CustomMapperStrategy(),
                new NullMappingStrategy(),
                new CacheMappingStrategy(),
                new NewObjectPropertyMapMappingStrategy(),
                new ExistingObjectMappingStrategy()
            };

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

		private class CacheMappingStrategy : ITypeMapObjectMapper
		{
			public object Map(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return context.InstanceCache[context];
			}

			public bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return context.DestinationValue == null && context.InstanceCache.ContainsKey(context);
			}
		}

		private abstract class PropertyMapMappingStrategy : ITypeMapObjectMapper
		{
			public object Map(ResolutionContext context, IMappingEngineRunner mapper)
			{
				var mappedObject = GetMappedObject(context, mapper);
				if (context.SourceValue != null)
                    context.InstanceCache[context] = mappedObject;

				context.TypeMap.BeforeMap(context.SourceValue, mappedObject);

                foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
                {
                    MapPropertyValue(context.CreatePropertyMapContext(propertyMap), mapper, mappedObject, propertyMap);
                }
                mappedObject = ReassignValue(context, mappedObject);

                context.TypeMap.AfterMap(context.SourceValue, mappedObject);

                return mappedObject;
			}

			protected virtual object ReassignValue(ResolutionContext context, object o)
			{
				return o;
			}

			public abstract bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper);

			protected abstract object GetMappedObject(ResolutionContext context, IMappingEngineRunner mapper);

			private void MapPropertyValue(ResolutionContext context, IMappingEngineRunner mapper, object mappedObject, PropertyMap propertyMap)
			{
				if (propertyMap.CanResolveValue())
                {
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

					object destinationValue = propertyMap.GetDestinationValue(mappedObject);;

					var sourceType = result.Type;
					var destinationType = propertyMap.DestinationProperty.MemberType;

					var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(result, destinationType);

					Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceType;

					var newContext = context.CreateMemberContext(typeMap, result.Value, destinationValue, targetSourceType,
																 propertyMap);

                    if (!propertyMap.ShouldAssignValue(newContext))
                        return;

                    // If condition succeeded and resolving failed, throw
                    if (resolvingExc != null)
                        throw resolvingExc;

                    try
					{
						object propertyValueToAssign = mapper.Map(newContext);

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
			}

			protected virtual void AssignValue(PropertyMap propertyMap, object mappedObject, object propertyValueToAssign)
			{
				if (propertyMap.CanBeSet)
					propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
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

		private class NewObjectPropertyMapMappingStrategy : PropertyMapMappingStrategy
		{
			public override bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return context.DestinationValue == null;
			}

			protected override object GetMappedObject(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return mapper.CreateObject(context);
			}
		}

		private class ExistingObjectMappingStrategy : PropertyMapMappingStrategy
		{
			public override bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return true;
			}

			protected override object GetMappedObject(ResolutionContext context, IMappingEngineRunner mapper)
			{
				return context.DestinationValue;
			}
		}
	}
}
