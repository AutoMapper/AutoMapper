namespace AutoMapper.Mappers
{
    using System.Reflection;

    public class AssignableArrayMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            if (context.SourceValue == null && !mapper.ShouldMapSourceCollectionAsNull(context))
            {
                return ObjectCreator.CreateObject(context.DestinationType);
            }

            return context.SourceValue;
        }

        public bool IsMatch(TypePair context, IConfigurationProvider configuration)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType)
                   && context.DestinationType.IsArray
                   && context.SourceType.IsArray
                   && !ElementsExplicitlyMapped(context, configuration)
                   ;
        }

        private bool ElementsExplicitlyMapped(TypePair context, IConfigurationProvider configuration)
        {
            var sourceElementType = context.SourceType.GetElementType();
            var destinationElementType = context.DestinationType.GetElementType();
            return configuration.FindTypeMapFor(sourceElementType, destinationElementType) != null;
        }
    }
}