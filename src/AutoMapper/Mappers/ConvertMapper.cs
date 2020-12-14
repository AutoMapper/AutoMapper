using System;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    public class ConvertMapper : IObjectMapper
    {
        public static bool IsPrimitive(Type type) => type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
        public bool IsMatch(in TypePair types) => IsPrimitive(types.SourceType) && IsPrimitive(types.DestinationType);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var convertMethod = typeof(Convert).GetMethod("To" + destExpression.Type.Name, new[] { sourceExpression.Type });
            return Call(convertMethod, sourceExpression);
        }
    }
}