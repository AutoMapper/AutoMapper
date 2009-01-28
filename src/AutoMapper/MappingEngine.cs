using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	public class MappingEngine : IMappingEngine
	{
		private readonly IConfiguration _configuration;

		public IConfiguration Configuration
		{
			get { return _configuration; }
		}

		public MappingEngine(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof(TSource);
			Type dtoType = typeof(TDestination);

			return (TDestination)Map(source, modelType, dtoType);
		}

		public object Map(object source, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = Configuration.FindTypeMapFor(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType);

			return Map(context);
		}

		private object Map(ResolutionContext context)
		{
			try
			{
				object valueToAssign;

				if (context.SourceValueTypeMap != null)
				{
					valueToAssign = CreateMappedObject(context);
				}
				else if (context.DestinationType == typeof(bool) && context.SourceType == typeof(bool?))
				{
					valueToAssign = context.SourceValue ?? false;
				}
				else if (context.SourceValue == null)
				{
					valueToAssign = CreateNullOrDefaultObject(context);
				}
				else if (context.DestinationType.Equals(typeof(string)))
				{
					valueToAssign = FormatDataElement(context);
				}
				else if (context.DestinationType.IsAssignableFrom(context.SourceType))
				{
					valueToAssign = context.SourceValue;
				}
				else if ((context.DestinationType.IsArray) && (context.SourceValue is IEnumerable))
				{
					valueToAssign = CreateArrayObject(context);
				}
				else
				{
					throw new AutoMapperMappingException(context);
				}

				return valueToAssign;
			}
			catch (Exception ex)
			{
				throw new AutoMapperMappingException(context, ex);
			}
		}

		private object CreateMappedObject(ResolutionContext context)
		{
			object dto = CreateObject(context.DestinationType);

			foreach (PropertyMap propertyMap in context.SourceValueTypeMap.GetPropertyMaps())
			{
				if (propertyMap.Ignored) continue;

				object modelMemberValue;
				Type modelMemberType;

				IValueResolver resolver = propertyMap.GetCustomValueResolver();

				if (resolver != null)
				{
					var inputValueToResolve = context.SourceValue;

					if (propertyMap.HasMembersToResolveForCustomResolver)
					{
						inputValueToResolve = ResolveModelMemberValue(propertyMap, context.SourceValue);
					}

					modelMemberValue = resolver.Resolve(inputValueToResolve);
					modelMemberType = modelMemberValue != null ? modelMemberValue.GetType() : typeof(object);
				}
				else
				{
					modelMemberValue = ResolveModelMemberValue(propertyMap, context.SourceValue);

					TypeMember modelMemberToUse = propertyMap.GetLastModelMemberInChain();
					modelMemberType = modelMemberToUse.GetMemberType();
				}

				var memberTypeMap = Configuration.FindTypeMapFor(modelMemberType, propertyMap.DestinationProperty.PropertyType);

				var newContext = context.CreateMemberContext(memberTypeMap, modelMemberValue, modelMemberType, propertyMap);

				object propertyValueToAssign = Map(newContext);

				propertyMap.DestinationProperty.SetValue(dto, propertyValueToAssign, new object[0]);
			}

			return dto;
		}

		private object CreateArrayObject(ResolutionContext context)
		{
			IEnumerable<object> enumerableValue = ((IEnumerable)context.SourceValue).Cast<object>();

			Type sourceElementType = GetElementType(context.SourceType);
			Type destElementType = context.DestinationType.GetElementType();

			Array dtoArrayValue = Array.CreateInstance(destElementType, enumerableValue.Count());

			int i = 0;
			foreach (object item in enumerableValue)
			{
				Type targetSourceType = sourceElementType;
				Type targetDestinationType = destElementType;

				if (item.GetType() != sourceElementType)
				{
					TypeMap itemTypeMap = Configuration.FindTypeMapFor(sourceElementType, destElementType);
					
					targetSourceType = item.GetType();
					targetDestinationType = itemTypeMap.GetDerivedTypeFor(targetSourceType);
				}

				TypeMap derivedTypeMap = Configuration.FindTypeMapFor(targetSourceType, targetDestinationType);

				var newContext = context.CreateElementContext(derivedTypeMap, item, targetSourceType, targetDestinationType, i);

				object mappedValue = Map(newContext);

				dtoArrayValue.SetValue(mappedValue, i);

				i++;
			}

			object valueToAssign = dtoArrayValue;
			return valueToAssign;
		}

		private object CreateNullOrDefaultObject(ResolutionContext context)
		{
			object valueToAssign;
			object nullValueToUse = null;

			if (context.PropertyMap != null)
				nullValueToUse = context.PropertyMap.GetNullSubstitute();

			if (context.DestinationType == typeof(string))
			{
				valueToAssign = FormatDataElement(context.CreateValueContext(nullValueToUse));
			}
			else if (context.DestinationType.IsArray)
			{
				Type elementType = context.DestinationType.GetElementType();
				Array arrayValue = Array.CreateInstance(elementType, 0);
				valueToAssign = arrayValue;
			}
			else if (nullValueToUse != null)
			{
				valueToAssign = nullValueToUse;
			}
			else
			{
				try
				{
					valueToAssign = Activator.CreateInstance(context.DestinationType, true);
				} catch (Exception e)
				{
					throw new Exception("Problem instantiating object of type "+context.DestinationType, e);
				}
			}

			return valueToAssign;
		}

		private string FormatDataElement(ResolutionContext context)
		{
			IValueFormatter valueFormatter = context.ContextTypeMap != null
												? Configuration.GetValueFormatter(context.ContextTypeMap.Profile)
			                                 	: Configuration.GetValueFormatter();

			return valueFormatter.FormatValue(context);
		}

		private static Type GetElementType(Type enumerableType)
		{
			if (enumerableType.HasElementType)
			{
				return enumerableType.GetElementType();
			}

			if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
			{
				return enumerableType.GetGenericArguments()[0];
			}

			Type ienumerableType = enumerableType.GetInterface("IEnumerable`1");
			if (ienumerableType != null)
			{
				return ienumerableType.GetGenericArguments()[0];
			}

			if (typeof(IEnumerable).IsAssignableFrom(enumerableType))
			{
				return typeof (object);
			}

			throw new ArgumentException(string.Format("Unable to find the element type for type '{0}'.", enumerableType), "enumerableType");
		}

		private static object ResolveModelMemberValue(PropertyMap propertyMap, object input)
		{
			object modelMemberValue = input;

			if (modelMemberValue != null)
			{
				foreach (TypeMember modelProperty in propertyMap.GetSourceMemberChain())
				{
					modelMemberValue = modelProperty.GetValue(modelMemberValue);

					if (modelMemberValue == null)
					{
						break;
					}
				}
			}
			return modelMemberValue;
		}

		private static object CreateObject(Type type)
		{
			return Activator.CreateInstance(type, true);
		}
	}
}