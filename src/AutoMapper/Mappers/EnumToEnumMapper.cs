using System;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    using static Execution.ExpressionBuilder;
    public class EnumToEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo ToObjectMethod = typeof(Enum).GetMethod("ToObject", new[] { typeof(Type), typeof(object) });
        private static readonly MethodInfo IsDefinedMethod = typeof(Enum).GetMethod("IsDefined", new[] { typeof(Type), typeof(object) });
        private static readonly MethodInfo TryParseMethod = typeof(Enum).StaticGenericMethod("TryParse", parametersCount: 3);
        public bool IsMatch(in TypePair context) => context.IsEnumToEnum();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationType = destExpression.Type;
            var sourceToObject = sourceExpression.ToObject();
            var toObject = Call(ToObjectMethod, Constant(destinationType), sourceToObject);
            var castToEnum = Convert(toObject, destinationType);
            var isDefined = Call(IsDefinedMethod, Constant(sourceExpression.Type), sourceToObject);
            var sourceToString = Call(sourceExpression, ObjectToString);
            var result = Variable(destinationType, "destinationEnumValue");
            var ignoreCase = True;
            var tryParse = Call(TryParseMethod.MakeGenericMethod(destinationType), sourceToString, ignoreCase, result);
            return Block(new[] { result }, Condition(isDefined, Condition(tryParse, result, castToEnum), castToEnum));
        }
    }
}