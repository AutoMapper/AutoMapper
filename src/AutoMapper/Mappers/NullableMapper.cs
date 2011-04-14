using System;

namespace AutoMapper.Mappers
{
	public class NullableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
            if (context == null) throw new ArgumentNullException("context");
            
			return context.SourceValue;
		}

		public bool IsMatch(ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

			return context.DestinationType.IsNullableType();
		}
	}
}