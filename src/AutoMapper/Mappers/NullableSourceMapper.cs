namespace AutoMapper.Internal.Mappers;

public class NullableSourceMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => context.SourceType.IsNullableType();
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
        configuration.MapExpression(profileMap, GetAssociatedTypes(sourceExpression.Type, destExpression.Type),
                ExpressionBuilder.Property(sourceExpression, "Value"), memberMap, destExpression);
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => GetAssociatedTypes(initialTypes.SourceType, initialTypes.DestinationType);
    static TypePair GetAssociatedTypes(Type sourceType, Type destinationType) => new(Nullable.GetUnderlyingType(sourceType), destinationType);
}