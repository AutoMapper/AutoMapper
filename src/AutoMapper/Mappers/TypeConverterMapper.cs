using System;
using System.ComponentModel;

namespace AutoMapper.Mappers
{
	public class TypeConverterMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			if (context.SourceValue == null)
			{
				return mapper.CreateObject(context);
			}
			Func<object> converter = GetConverter(context);
			return converter != null ? converter() : null;
		}
		private static Func<object> GetConverter(ResolutionContext context)
		{
			TypeConverter typeConverter = GetTypeConverter(context.SourceType);
			if (typeConverter.CanConvertTo(context.DestinationType))
				return () => typeConverter.ConvertTo(context.SourceValue, context.DestinationType);

			typeConverter = GetTypeConverter(context.DestinationType);
			if(typeConverter.CanConvertFrom(context.SourceType))
				return () => typeConverter.ConvertFrom(context.SourceValue);

			return null;
		}
		public bool IsMatch(ResolutionContext context)
		{
			return GetConverter(context) != null;
		}

		private static TypeConverter GetTypeConverter(Type type)
		{
#if !SILVERLIGHT
			return TypeDescriptor.GetConverter(type);
#else
			var attributes = type.GetCustomAttributes(typeof(TypeConverterAttribute), false);

			if (attributes.Length != 1)
				return new TypeConverter();

			var converterAttribute = (TypeConverterAttribute)attributes[0];
			var converterType = Type.GetType(converterAttribute.ConverterTypeName);

			if (converterType == null)
                return new TypeConverter();

			return Activator.CreateInstance(converterType) as TypeConverter;
#endif
		}
	}
}