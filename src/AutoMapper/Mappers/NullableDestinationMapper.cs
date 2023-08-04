namespace AutoMapper.Internal.Mappers;

public class NullableDestinationMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => context.DestinationType.IsNullableType();
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
        configuration.MapExpression(profileMap, GetAssociatedTypes(sourceExpression.Type, destExpression.Type), sourceExpression, memberMap);
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => GetAssociatedTypes(initialTypes.SourceType, initialTypes.DestinationType);
    static TypePair GetAssociatedTypes(Type sourceType, Type destinationType) => new(sourceType, Nullable.GetUnderlyingType(destinationType));
}