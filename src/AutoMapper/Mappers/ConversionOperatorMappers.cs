using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class ConversionOperatorMapper : IObjectMapper
    {
        private readonly string _operatorName;
        public ConversionOperatorMapper(string operatorName) => _operatorName = operatorName;
        public bool IsMatch(in TypePair context) => context.GetConversionOperator(_operatorName) != null;
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var implicitOperator = new TypePair(sourceExpression.Type, destExpression.Type).GetConversionOperator(_operatorName);
            return Expression.Call(implicitOperator, sourceExpression);
        }
    }
}