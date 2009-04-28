using System;
using System.ComponentModel;

namespace AutoMapper.Mappers
{
	public class TypeConverterMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
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
			return context.SourceValue == null
			       	? TypeDescriptor.GetConverter(context.SourceType)
			       	: TypeDescriptor.GetConverter(context.SourceValue);
		}
	}
}