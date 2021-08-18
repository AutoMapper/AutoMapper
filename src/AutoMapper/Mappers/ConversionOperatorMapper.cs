using System;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper.Internal.Mappers
{
    using static Execution.ExpressionBuilder;
    public class ConversionOperatorMapper : IObjectMapper
    {
        private readonly string _operatorName;
        public ConversionOperatorMapper(string operatorName) => _operatorName = operatorName;
        public bool IsMatch(TypePair context) => GetConversionOperator(context.SourceType, context.DestinationType) != null;
        private MethodInfo GetConversionOperator(Type sourceType, Type destinationType)
        {
            foreach (MethodInfo sourceMethod in sourceType.GetMember(_operatorName, MemberTypes.Method, (TypeExtensions.StaticFlags & ~BindingFlags.DeclaredOnly) | BindingFlags.FlattenHierarchy))
            {
                if (destinationType.IsAssignableFrom(sourceMethod.ReturnType))
                {
                    return sourceMethod;
                }
            }
            return destinationType.GetMethod(_operatorName, TypeExtensions.StaticFlags, null, new[] { sourceType }, null);
        }
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var conversionOperator = GetConversionOperator(sourceExpression.Type, destExpression.Type);
            return Expression.Call(conversionOperator, ToType(sourceExpression, conversionOperator.GetParameters()[0].ParameterType));
        }
    }
}