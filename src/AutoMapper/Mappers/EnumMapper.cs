using System;

namespace AutoMapper.Mappers
{
	public class EnumMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			Type enumDestType = TypeHelper.GetEnumerationType(context.DestinationType);

			if (!enumDestType.IsEnum)
			{
				throw new AutoMapperMappingException(context, "Cannot map an Enum source type to a non-Enum destination type.");
			}

			return Enum.Parse(enumDestType, Enum.GetName(context.SourceType, context.SourceValue));
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.SourceType.IsEnum;
		}
	}
}