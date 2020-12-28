using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    public class ToStringMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.DestinationType == typeof(string);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var sourceType = sourceExpression.Type;
            Expression toStringCall = Call(sourceExpression, ExpressionFactory.ObjectToString);
            return sourceType.IsEnum ? EnumToString(sourceExpression, sourceType, toStringCall) : toStringCall;
        }
        private static Expression EnumToString(Expression sourceExpression, Type sourceType, Expression toStringCall)
        {
            List<SwitchCase> switchCases = null;
            foreach (var memberInfo in sourceType.GetFields())
            {
                var attributeValue = memberInfo.GetCustomAttribute<EnumMemberAttribute>()?.Value;
                if (attributeValue != null)
                {
                    var switchCase = SwitchCase(Constant(attributeValue), Constant(Enum.ToObject(sourceType, memberInfo.GetValue(null))));
                    switchCases ??= new();
                    switchCases.Add(switchCase);
                }
            }
            return switchCases != null ? Switch(sourceExpression, toStringCall, null, switchCases) : toStringCall;
        }
    }
}