namespace AutoMapper.Mappers
{
    using System.Reflection;

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
            return context.DestinationType.IsAssignableFrom(context.SourceType);
        }
    }
}