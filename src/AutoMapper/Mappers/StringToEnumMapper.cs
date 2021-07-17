using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
namespace AutoMapper.Internal.Mappers
{
    using static Execution.ExpressionBuilder;
    using static Expression;
    public class StringToEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EqualsMethod = typeof(StringToEnumMapper).GetMethod("StringCompareOrdinalIgnoreCase");
        private static readonly MethodInfo ParseMethod = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) });
        private static readonly MethodInfo IsNullOrEmptyMethod = typeof(string).GetMethod("IsNullOrEmpty");
        public bool IsMatch(TypePair context) => context.SourceType == typeof(string) && context.DestinationType.IsEnum;
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationType = destExpression.Type;
            List<SwitchCase> switchCases = null;
            foreach (var memberInfo in destinationType.GetFields(TypeExtensions.StaticFlags))
            {
                var attributeValue = memberInfo.GetCustomAttribute<EnumMemberAttribute>()?.Value;
                if (attributeValue != null)
                {
                    var switchCase = SwitchCase(
                        ToType(Constant(Enum.ToObject(destinationType, memberInfo.GetValue(null))), destinationType), Constant(attributeValue));
                    switchCases ??= new();
                    switchCases.Add(switchCase);
                }
            }
            var enumParse = ToType(Call(ParseMethod, Constant(destinationType), sourceExpression, True), destinationType);
            var parse = switchCases != null ? Switch(sourceExpression, enumParse, EqualsMethod, switchCases) : enumParse;
            return Condition(Call(IsNullOrEmptyMethod, sourceExpression), Default(destinationType), parse);
        }
        public static bool StringCompareOrdinalIgnoreCase(string x, string y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);
    }
}