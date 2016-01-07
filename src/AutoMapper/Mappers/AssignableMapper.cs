namespace AutoMapper.Mappers
{
    using Internal;
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

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType);
        }
    }
}