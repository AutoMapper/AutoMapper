using AutoMapper.Internal;
using System.Linq.Expressions;
namespace AutoMapper
{
    using Execution;
    public class KeyValueMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsKeyValue() && context.DestinationType.IsKeyValue();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var sourceType = sourceExpression.Type;
            var destinationType = destExpression.Type;
            var typePairKey = new TypePair(sourceType.GenericTypeArguments[0], destinationType.GenericTypeArguments[0]);
            var typePairValue = new TypePair(sourceType.GenericTypeArguments[1], destinationType.GenericTypeArguments[1]);
            var keyExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, typePairKey, ExpressionFactory.Property(sourceExpression, "Key"));
            var valueExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, typePairValue, ExpressionFactory.Property(sourceExpression, "Value"));
            return Expression.New(destinationType.GetConstructors()[0], keyExpr, valueExpr);
        }
    }
}