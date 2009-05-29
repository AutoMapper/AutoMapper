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
				return context.DestinationValue ?? mapper.CreateObject(context.DestinationType);
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
			return TypeDescriptor.GetConverter(context.SourceType);
		}
	}
}