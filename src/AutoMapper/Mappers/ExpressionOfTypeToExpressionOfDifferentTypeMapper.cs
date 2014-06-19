using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Mappers
{
    public class ExpressionOfTypeToExpressionOfDifferentTypeMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var srcExpressArgType = context.SourceType.GetSinglePredicateExpressionArgumentType();
            var destExpressArgType = context.DestinationType.GetSinglePredicateExpressionArgumentType();

            var typeMap = Mapper.FindTypeMapFor(destExpressArgType, srcExpressArgType);
            return GenerateEquivilentExpressionFromTypeMap.GetExpression(typeMap, context.SourceValue);
        }

        public bool IsMatch(ResolutionContext context)
        {
            var srcExpressArgType = context.SourceType.GetSinglePredicateExpressionArgumentType();
            if (srcExpressArgType == null)
                return false;
            var destExpressArgType = context.DestinationType.GetSinglePredicateExpressionArgumentType();
            if (destExpressArgType == null)
                return false;

            var mapper = Mapper.FindTypeMapFor(destExpressArgType, srcExpressArgType);
            if (mapper == null)
                return false;

            return true;
        }
    }
}