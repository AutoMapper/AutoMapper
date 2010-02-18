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

			TypeConverter typeConverter = GetTypeConverter(context);
			return typeConverter.ConvertTo(context.SourceValue, context.DestinationType);
		}

		public bool IsMatch(ResolutionContext context)
		{
			TypeConverter typeConverter = GetTypeConverter(context);
			return typeConverter.CanConvertTo(context.DestinationType);
		}

		private static TypeConverter GetTypeConverter(ResolutionContext context)
		{
#if !SILVERLIGHT
            return TypeDescriptor.GetConverter(context.SourceType);
#else
			var attributes = context.SourceType.GetCustomAttributes(typeof(TypeConverterAttribute), false);

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