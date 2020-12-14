using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class EnumToStringMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.DestinationType == typeof(string) && context.SourceType.IsEnum;
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var sourceType = sourceExpression.Type;
            var toStringCall = Expression.Call(sourceExpression, ExpressionFactory.ObjectToString);
            var switchCases = new List<SwitchCase>();
            foreach (var memberInfo in sourceType.GetFields())
            {
                var attribute = memberInfo.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                if (attribute?.Value != null)
                {
                    var switchCase = Expression.SwitchCase(Expression.Constant(attribute.Value),
                        Expression.Constant(Enum.ToObject(sourceType, memberInfo.GetMemberValue(null))));
                    switchCases.Add(switchCase);
                }
            }
            return switchCases.Count > 0
                ? (Expression) Expression.Switch(sourceExpression, toStringCall, switchCases.ToArray())
                : toStringCall;
        }
    }
}