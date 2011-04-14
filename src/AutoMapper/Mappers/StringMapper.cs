using System;

namespace AutoMapper.Mappers
{
	public class StringMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
            if (context == null) throw new ArgumentNullException("context");
            if (mapper == null) throw new ArgumentNullException("mapper");

			if (context.SourceValue == null)
			{
				return mapper.FormatValue(context.CreateValueContext(null));
			}
			return mapper.FormatValue(context);
		}

		public bool IsMatch(ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

			return context.DestinationType.Equals(typeof(string));
		}
	}
}