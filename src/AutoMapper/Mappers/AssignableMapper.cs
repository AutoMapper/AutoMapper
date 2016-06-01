using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public class AssignableMapper : IObjectMapExpression
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

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return sourceExpression;
        }
    }
}