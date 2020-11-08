using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static ExpressionFactory;

    public class StringToEnumMapper : IObjectMapper
    {
        private static readonly MethodInfo EqualsMethodInfo = typeof(StringToEnumMapper).GetMethod("StringCompareOrdinalIgnoreCase");
        public bool IsMatch(in TypePair context) => context.SourceType == typeof(string) && context.DestinationType.IsEnum;
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression)
        {
            var destinationType = destExpression.Type;
            var enumParse = Expression.Call(typeof(Enum), "Parse", null, Expression.Constant(destinationType),
                sourceExpression, True);
            var switchCases = new List<SwitchCase>();
            foreach (var memberInfo in destinationType.GetFields())
            {
                var attribute = memberInfo.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                if (attribute?.Value != null)
                {
                    var switchCase = Expression.SwitchCase(
                        ToType(Expression.Constant(Enum.ToObject(destinationType, memberInfo.GetMemberValue(null))),
                            destinationType), Expression.Constant(attribute.Value));
                    switchCases.Add(switchCase);
                }
            }
            var switchTable = switchCases.Count > 0
                ? Expression.Switch(sourceExpression, ToType(enumParse, destinationType), EqualsMethodInfo, switchCases)
                : ToType(enumParse, destinationType);
            var isNullOrEmpty = Expression.Call(typeof(string), "IsNullOrEmpty", null, sourceExpression);
            return Expression.Condition(isNullOrEmpty, Expression.Default(destinationType), switchTable);
        }
        public static bool StringCompareOrdinalIgnoreCase(string x, string y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);
    }
}