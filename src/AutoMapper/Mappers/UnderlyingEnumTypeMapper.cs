using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers
{
    public class UnderlyingTypeEnumMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.IsEnumToUnderlyingType() || context.IsUnderlyingTypeToEnum();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression) => sourceExpression;
    }
}