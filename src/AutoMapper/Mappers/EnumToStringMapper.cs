using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using AutoMapper.Internal;
namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    public class EnumToStringMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.DestinationType == typeof(string) && context.SourceType.IsEnum;
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var sourceType = sourceExpression.Type;
            var switchCases = new List<SwitchCase>();
            foreach (var memberInfo in sourceType.GetFields())
            {
                var attributeValue = memberInfo.GetCustomAttribute<EnumMemberAttribute>()?.Value;
                if (attributeValue != null)
                {
                    var switchCase = SwitchCase(Constant(attributeValue), Constant(Enum.ToObject(sourceType, memberInfo.GetValue(null))));
                    switchCases.Add(switchCase);
                }
            }
            Expression toStringCall = Call(sourceExpression, ExpressionFactory.ObjectToString);
            return switchCases.Count > 0 ? Switch(sourceExpression, toStringCall, switchCases.ToArray()) : toStringCall;
        }
    }
}