namespace AutoMapper.Mappers
{
    using System.Reflection;

    public class AssignableArrayMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            if (context.SourceValue == null && !mapper.ShouldMapSourceCollectionAsNull(context))
            {
                return mapper.CreateObject(context);
            }

            return context.SourceValue;
        }

        public bool IsMatch(ResolutionContext context)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType)
                   && context.DestinationType.IsArray
                   && context.SourceType.IsArray;
        }
    }
}