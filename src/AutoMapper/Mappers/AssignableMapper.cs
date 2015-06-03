using System.Reflection;

namespace AutoMapper.Mappers
{
    using Internal;
    public class AssignableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			if (context.SourceValue == null && !mapper.ShouldMapSourceValueAsNull(context))
			{
				return mapper.CreateObject(context);
			}

			return context.SourceValue;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return (context.DestinationValue == null || context.Parent != null)
				&& context.DestinationType.IsAssignableFrom(context.SourceType);
		}
	}

}