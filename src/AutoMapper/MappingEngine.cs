using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
			Type modelType = typeof (TSource);
			Type destinationType = typeof (TDestination);

			return (TDestination) Map(source, modelType, destinationType);
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			Type modelType = typeof (TSource);
			Type destinationType = typeof (TDestination);

			return (TDestination) Map(source, destination, modelType, destinationType);
		}

		public object Map(object source, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = Configuration.FindTypeMapFor(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType);

			return Map(context);
		}

		public object Map(object source, object destination, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = Configuration.FindTypeMapFor(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType);

			return Map(context);
		}

		private object Map(ResolutionContext context)
		{
			try
			{
				object valueToAssign;

				if (context.SourceValueTypeMap != null && context.SourceValueTypeMap.CustomMapper != null)
				{
					valueToAssign = context.SourceValueTypeMap.CustomMapper(context.SourceValue);
				}
				else if (context.SourceValueTypeMap != null)
				{
					valueToAssign = CreateMappedObject(context);
				}
				else if (context.SourceType.IsEnum)
				{
					valueToAssign = MapEnumSource(context);
				}
				else if (context.SourceValue == null)
				{
					valueToAssign = CreateNullOrDefaultObject(context);
				}
				else if (context.DestinationType.Equals(typeof (string)))
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
				else if (context.SourceType.IsNullableType())
				{
					valueToAssign = MapNullableType(context);
				}
				else
				{
					throw new AutoMapperMappingException(context, "Missing type map configuration or unsupported mapping.");
				}

				return valueToAssign;
			}
			catch (Exception ex)
			{
				throw new AutoMapperMappingException(context, ex);
			}
		}

		private object MapNullableType(ResolutionContext context)
		{
			PropertyInfo hasValueProp = context.SourceType.GetProperty("HasValue", BindingFlags.Public | BindingFlags.ExactBinding | BindingFlags.Instance);
			PropertyInfo valueProp = context.SourceType.GetProperty("Value", BindingFlags.Public | BindingFlags.ExactBinding | BindingFlags.Instance);
			Type sourceType = context.SourceType.GetGenericArguments()[0];

			var hasValue = (bool) hasValueProp.GetValue(context.SourceValue, new object[0]);
			object value = null;

			if (hasValue)
			{
				value = valueProp.GetValue(context.SourceValue, new object[0]);
			}

			var newContext = context.CreateValueContext(value, sourceType);

			return Map(newContext);
		}

		private static object MapEnumSource(ResolutionContext context)
		{
			if (context.DestinationType == typeof (string))
				return context.SourceValue.ToString();

			if (context.DestinationType.IsNullableType() && context.SourceValue == null)
				return null;

			if (!context.DestinationType.IsEnum)
				throw new AutoMapperMappingException(context, "Cannot map an Enum source type to a non-Enum destination type.");

			if (context.SourceValue == null)
				return CreateObject(context.DestinationType);

			return Enum.Parse(context.DestinationType, Enum.GetName(context.SourceType, context.SourceValue));
		}

		private object CreateMappedObject(ResolutionContext context)
		{
			object mappedObject = context.DestinationValue ?? CreateObject(context.DestinationType);

			foreach (PropertyMap propertyMap in context.SourceValueTypeMap.GetPropertyMaps())
			{
				if (!propertyMap.CanResolveValue())
				{
					continue;
				}

				var result = propertyMap.ResolveValue(context.SourceValue);

				var memberTypeMap = Configuration.FindTypeMapFor(result.Type, propertyMap.DestinationProperty.PropertyType);

				var newContext = context.CreateMemberContext(memberTypeMap, result.Value, result.Type, propertyMap);

				object propertyValueToAssign = Map(newContext);

				propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign, new object[0]);
			}

			return mappedObject;
		}

		private object CreateArrayObject(ResolutionContext context)
		{
			IEnumerable<object> enumerableValue = ((IEnumerable) context.SourceValue).Cast<object>();

			Type sourceElementType = GetElementType(context.SourceType);
			Type destElementType = context.DestinationType.GetElementType();

			Array destArray = Array.CreateInstance(destElementType, enumerableValue.Count());

			int i = 0;
			foreach (object item in enumerableValue)
			{
				Type targetSourceType = sourceElementType;
				Type targetDestinationType = destElementType;

				if (item.GetType() != sourceElementType)
				{
					targetSourceType = item.GetType();

					TypeMap itemTypeMap =
						Configuration.FindTypeMapFor(sourceElementType, destElementType)
						?? Configuration.FindTypeMapFor(targetSourceType, destElementType);

					targetDestinationType = itemTypeMap.GetDerivedTypeFor(targetSourceType);
				}

				TypeMap derivedTypeMap = Configuration.FindTypeMapFor(targetSourceType, targetDestinationType);

				var newContext = context.CreateElementContext(derivedTypeMap, item, targetSourceType, targetDestinationType, i);

				object mappedValue = Map(newContext);

				destArray.SetValue(mappedValue, i);

				i++;
			}

			object valueToAssign = destArray;
			return valueToAssign;
		}

		private object CreateNullOrDefaultObject(ResolutionContext context)
		{
			object valueToAssign;

			if (context.DestinationType == typeof (string))
			{
				valueToAssign = FormatDataElement(context.CreateValueContext(null));
			}
			else if (context.DestinationType.IsArray)
			{
				Type elementType = context.DestinationType.GetElementType();
				Array arrayValue = Array.CreateInstance(elementType, 0);
				valueToAssign = arrayValue;
			}
			else
			{
				valueToAssign = context.DestinationValue ?? Activator.CreateInstance(context.DestinationType, true);
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

			if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition().Equals(typeof (IEnumerable<>)))
			{
				return enumerableType.GetGenericArguments()[0];
			}

			Type ienumerableType = enumerableType.GetInterface("IEnumerable`1");
			if (ienumerableType != null)
			{
				return ienumerableType.GetGenericArguments()[0];
			}

			if (typeof (IEnumerable).IsAssignableFrom(enumerableType))
			{
				return typeof (object);
			}

			throw new ArgumentException(string.Format("Unable to find the element type for type '{0}'.", enumerableType), "enumerableType");
		}

		private static object CreateObject(Type type)
		{
			return Activator.CreateInstance(type, true);
		}
	}
}