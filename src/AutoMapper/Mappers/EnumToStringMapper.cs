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
    public class EnumToStringMapper : IObjectMapper
    {    
        public bool IsMatch(TypePair context) => context.DestinationType == typeof(string) && ElementTypeHelper.GetEnumerationType(context.SourceType) != null;

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceType = sourceExpression.Type;
            var sourceTypeEnum = ElementTypeHelper.GetEnumerationType(sourceType);
            var toStringCall = Expression.Call(sourceExpression, typeof(object).GetDeclaredMethod("ToString"));
            return BuildEnumMemberSwitchTable(sourceTypeEnum, sourceExpression, toStringCall);
        }

        private static Expression BuildEnumMemberSwitchTable(Type destinationEnumType, Expression sourceExpression, Expression toStringCall)
        {
            var switchCases = new List<SwitchCase>();
            var enumNames = destinationEnumType.GetDeclaredMembers();
            var resultVariable = Expression.Variable(typeof(string));
            Expression resultExpression;
            foreach (var memberInfo in enumNames.Where(x => x.IsStatic()))
            {
                var attribute = memberInfo.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                if (attribute?.Value != null)
                {
                    var switchCase = Expression.SwitchCase(
                        Expression.Assign(resultVariable, Expression.Constant(attribute.Value)),
                        Expression.Constant(Enum.ToObject(destinationEnumType, memberInfo.GetMemberValue(null))));
                    switchCases.Add(switchCase);
                }
            }
            if (switchCases.Count > 0)
            {
                var returnTarget = Expression.Label(typeof(string));
                var blockExpressions = new List<Expression>
                {
                    Expression.Switch(sourceExpression, Expression.Assign(resultVariable, toStringCall), switchCases.ToArray()),
                    Expression.Label(returnTarget, resultVariable)
                };
                resultExpression = Expression.Block(new[] { resultVariable }, blockExpressions);
            }
            else
            {
                resultExpression = toStringCall;
            }
            return resultExpression;
        }
    }
}
