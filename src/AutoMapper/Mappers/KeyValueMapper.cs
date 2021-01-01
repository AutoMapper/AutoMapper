using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers
{
    using Execution;
    public class KeyValueMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsKeyValue() && context.DestinationType.IsKeyValue();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var sourceType = sourceExpression.Type;
            var destinationType = destExpression.Type;
            var typePairKey = new TypePair(sourceType.GenericTypeArguments[0], destinationType.GenericTypeArguments[0]);
            var typePairValue = new TypePair(sourceType.GenericTypeArguments[1], destinationType.GenericTypeArguments[1]);
            var keyExpr = configurationProvider.MapExpression(profileMap, typePairKey, ExpressionBuilder.Property(sourceExpression, "Key"));
            var valueExpr = configurationProvider.MapExpression(profileMap, typePairValue, ExpressionBuilder.Property(sourceExpression, "Value"));
            return Expression.New(destinationType.GetConstructors()[0], keyExpr, valueExpr);
        }
    }
}