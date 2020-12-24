using AutoMapper.Internal;
using System.Linq.Expressions;
namespace AutoMapper.Mappers
{
    public class ConstructorMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.DestinationType.GetConstructor(new[] { context.SourceType }) != null;
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var constructor = destExpression.Type.GetConstructor(new[] { sourceExpression.Type });
            var parameterType = constructor.GetParameters()[0].ParameterType;
            return Expression.New(constructor, ExpressionFactory.ToType(sourceExpression, parameterType));
        }
    }
}