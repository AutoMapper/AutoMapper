namespace AutoMapper.Mappers
{
    using System.Reflection;

    public class AssignableMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            if (context.SourceValue == null && !context.Mapper.ShouldMapSourceValueAsNull(context))
            {
                return context.Mapper.CreateObject(context);
            }

            return context.SourceValue;
        }

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType);
        }
    }
}