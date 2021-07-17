using System;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper.Internal.Mappers
{
    using static Execution.ExpressionBuilder;
    public class ConstructorMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => GetConstructor(context.SourceType, context.DestinationType) != null;
        private static ConstructorInfo GetConstructor(Type sourceType, Type destinationType) => 
            destinationType.GetConstructor(TypeExtensions.InstanceFlags, null, new[] { sourceType }, null);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var constructor = GetConstructor(sourceExpression.Type, destExpression.Type);
            return Expression.New(constructor, ToType(sourceExpression, constructor.GetParameters()[0].ParameterType));
        }
    }
}