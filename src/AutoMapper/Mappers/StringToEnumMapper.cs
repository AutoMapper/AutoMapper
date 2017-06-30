using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using AutoMapper.Internal;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static ExpressionFactory;

    public class StringToEnumMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.SourceType == typeof(string) &&
                                                 ElementTypeHelper.GetEnumerationType(context.DestinationType) != null;

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression)
        {
            var destinationType = destExpression.Type;
            var destinationEnumType = ElementTypeHelper.GetEnumerationType(destinationType);
            var enumParse = Expression.Call(typeof(Enum), "Parse", null, Expression.Constant(destinationEnumType),
                sourceExpression, Expression.Constant(true));
            var switchCases = new List<SwitchCase>();
            var enumNames = destinationEnumType.GetDeclaredMembers();
            foreach (var memberInfo in enumNames.Where(x => x.IsStatic()))
            {
                var attribute = memberInfo.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                if (attribute?.Value != null)
                {
                    var switchCase = Expression.SwitchCase(
                        ToType(Expression.Constant(Enum.ToObject(destinationEnumType, memberInfo.GetMemberValue(null))),
                            destinationType), Expression.Constant(attribute.Value));
                    switchCases.Add(switchCase);
                }
            }
            var equalsMethodInfo = Method(() => StringCompareOrdinalIgnoreCase(null, null));
            var switchTable = switchCases.Count > 0
                ? Expression.Switch(sourceExpression, ToType(enumParse, destinationType), equalsMethodInfo, switchCases)
                : ToType(enumParse, destinationType);
            var isNullOrEmpty = Expression.Call(typeof(string), "IsNullOrEmpty", null, sourceExpression);
            return Expression.Condition(isNullOrEmpty, Expression.Default(destinationType), switchTable);
        }

        private static bool StringCompareOrdinalIgnoreCase(string x, string y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x, y);
        }
    }
}