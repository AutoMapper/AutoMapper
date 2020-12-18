using System;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    public class EnumToEnumMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.IsEnumToEnum();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationType = destExpression.Type;
            var sourceToObject = sourceExpression.ToObject();
            var toObject = ExpressionFactory.Call(typeof(Enum), "ToObject", null, Constant(destinationType), sourceToObject);
            var castToObject = Convert(toObject, destinationType);
            var isDefined = ExpressionFactory.Call(typeof(Enum), "IsDefined", null, Constant(sourceExpression.Type), sourceToObject);
            var sourceToString = Call(sourceExpression, ExpressionFactory.ObjectToString);
            var result = Variable(destinationType, "destinationEnumValue");
            var ignoreCase = ExpressionFactory.True;
            var tryParse = ExpressionFactory.Call(typeof(Enum), "TryParse", new[] { destinationType }, sourceToString, ignoreCase, result);
            return Block(new[] { result }, Condition(isDefined, Condition(tryParse, result, castToObject), castToObject));
        }
    }
}