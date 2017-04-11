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
        public bool IsMatch(TypePair context)
        {
            var destEnumType = ElementTypeHelper.GetEnumerationType(context.DestinationType);
            return destEnumType != null && context.SourceType == typeof(string);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var destinationType = destExpression.Type;
            var destinationEnumType = ElementTypeHelper.GetEnumerationType(destinationType);
            var enumParse = Expression.Call(typeof(Enum), "Parse", null, Expression.Constant(destinationEnumType), sourceExpression, Expression.Constant(true));
            var switchTable = BuildEnumMemberSwitchTable(destinationEnumType, destinationType, sourceExpression, enumParse);
            var isNullOrEmpty = Expression.Call(typeof(string), "IsNullOrEmpty", null, sourceExpression);
            return Expression.Condition(isNullOrEmpty, Expression.Default(destinationType), switchTable);
        }

        private static Expression BuildEnumMemberSwitchTable(Type destinationEnumType, Type destinationType, Expression sourceExpression, Expression enumParse)
        {
            var switchCases = new List<SwitchCase>();
            var enumNames = destinationEnumType.GetDeclaredMembers();
            var resultVariable = Expression.Variable(destinationEnumType);
            sourceExpression = Expression.Call(sourceExpression, "ToUpperInvariant", null, null);
            Expression resultExpression;
            foreach (var memberInfo in enumNames.Where(x => x.IsStatic()))
            {
                var attribute = memberInfo.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                if (attribute?.Value != null)
                {
                    var switchCase = Expression.SwitchCase(
                        Expression.Assign(resultVariable, Expression.Constant(Enum.ToObject(destinationEnumType, memberInfo.GetMemberValue(null)))),
                        Expression.Constant(attribute.Value.ToUpperInvariant()));
                    switchCases.Add(switchCase);
                }
            }
            if (switchCases.Count > 0)
            {
                var returnTarget = Expression.Label(destinationType);
                var blockExpressions = new List<Expression>
                {
                    Expression.Switch(sourceExpression, Expression.Assign(resultVariable, ToType(enumParse, destinationEnumType)), switchCases.ToArray()),
                    Expression.Label(returnTarget, ToType(resultVariable, destinationType))
                };
                resultExpression = Expression.Block(new[] {resultVariable}, blockExpressions);
            }
            else
            {
                resultExpression = ToType(enumParse, destinationType);
            }
            return resultExpression;
        }
    }
}