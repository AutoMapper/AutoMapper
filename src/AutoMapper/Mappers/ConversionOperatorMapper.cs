using System;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper.Internal.Mappers
{
    public class ConversionOperatorMapper : IObjectMapper
    {
        private readonly string _operatorName;
        public ConversionOperatorMapper(string operatorName) => _operatorName = operatorName;
        public bool IsMatch(in TypePair context) => GetConversionOperator(context.SourceType, context.DestinationType) != null;
        private MethodInfo GetConversionOperator(Type sourceType, Type destinationType)
        {
            foreach (MethodInfo sourceMethod in sourceType.GetMember(_operatorName, MemberTypes.Method, TypeExtensions.StaticFlags))
            {
                if (sourceMethod.ReturnType == destinationType)
                {
                    return sourceMethod;
                }
            }
            return destinationType.GetMethod(_operatorName, TypeExtensions.StaticFlags, null, new[] { sourceType }, null);
        }
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            Expression.Call(GetConversionOperator(sourceExpression.Type, destExpression.Type), sourceExpression);
    }
}