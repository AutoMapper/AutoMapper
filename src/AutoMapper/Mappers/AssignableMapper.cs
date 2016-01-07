namespace AutoMapper.Mappers
{
    using Internal;
    using System.Reflection;

    public class AssignableMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            if (context.SourceValue == null && !context.Engine.ShouldMapSourceValueAsNull(context))
            {
                return context.Engine.CreateObject(context);
            }

            return context.SourceValue;
        }

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType);
        }
    }
}