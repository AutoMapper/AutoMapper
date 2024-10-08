namespace AutoMapper.Internal.Mappers;

public sealed class UnderlyingTypeEnumMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => context.IsEnumToUnderlyingType() || context.IsUnderlyingTypeToEnum();
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression) => sourceExpression;
}