using System;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    using static Execution.ExpressionBuilder;
    public class EnumToEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo TryParseMethod = typeof(Enum).StaticGenericMethod("TryParse", parametersCount: 3);
        public bool IsMatch(in TypePair context) => context.IsEnumToEnum();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationType = destExpression.Type;
            var sourceToString = Call(sourceExpression, ObjectToString);
            var result = Variable(destinationType, "destinationEnumValue");
            var ignoreCase = True;
            var tryParse = Call(TryParseMethod.MakeGenericMethod(destinationType), sourceToString, ignoreCase, result);
            return Block(new[] { result }, Condition(tryParse, result, Convert(sourceExpression, destinationType)));
        }
    }
}