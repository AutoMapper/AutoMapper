using System;

namespace AutoMapper.Mappers
{
	public class EnumMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			Type enumDestType = TypeHelper.GetEnumerationType(context.DestinationType);

			if (context.SourceValue == null)
			{
				return context.DestinationValue ?? mapper.CreateObject(context.DestinationType);
			}

			return Enum.Parse(enumDestType, Enum.GetName(context.SourceType, context.SourceValue));
		}

		public bool IsMatch(ResolutionContext context)
		{
			var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
			var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

			return sourceEnumType != null
				&& destEnumType != null;
		}
	}
}