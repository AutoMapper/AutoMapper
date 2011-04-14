using System;

namespace AutoMapper.Mappers
{
	public class AssignableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
            if (context == null) throw new ArgumentNullException("context");
            if (mapper == null) throw new ArgumentNullException("mapper");

			if (context.SourceValue == null && !mapper.ShouldMapSourceValueAsNull(context))
			{
				return mapper.CreateObject(context);
			}

			return context.SourceValue;
		}

		public bool IsMatch(ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

			return context.DestinationType.IsAssignableFrom(context.SourceType);
		}
	}

}