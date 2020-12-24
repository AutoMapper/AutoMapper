using AutoMapper.Internal;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public class ConstructorMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => GetConstructor(context.SourceType, context.DestinationType) != null;
        private static ConstructorInfo GetConstructor(Type sourceType, Type destinationType) => 
            destinationType.GetConstructor(TypeExtensions.InstanceFlags | BindingFlags.ExactBinding, null, new[] { sourceType }, null);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            Expression.New(GetConstructor(sourceExpression.Type, destExpression.Type), sourceExpression);
    }
}