using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    public class ArrayCopyMapper : ArrayMapper
    {
        private static readonly MethodInfo ArrayCopyMethod = typeof(Array).GetMethod("Copy", new[] { typeof(Array), typeof(Array), typeof(long) });
        private static readonly PropertyInfo ArrayLengthProperty = typeof(Array).GetProperty("LongLength");
        public override bool IsMatch(in TypePair context)
        {
            if (!context.IsArray)
            {
                return false;
            }
            var sourceElementType = context.SourceType.GetElementType();
            return sourceElementType.IsPrimitive && sourceElementType == context.DestinationType.GetElementType();
        }
        public override Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destElementType = destExpression.Type.GetElementType();
            var sourceElementType = sourceExpression.Type.GetElementType();
            if (configurationProvider.FindTypeMapFor(sourceElementType, destElementType) != null)
            {
                return base.MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression);
            }
            var dest = Parameter(destExpression.Type, "destArray");
            var sourceLength = Parameter(ArrayLengthProperty.PropertyType, "sourceLength");
            return Block(
                new[] {dest, sourceLength},
                Assign(sourceLength, Property(sourceExpression, ArrayLengthProperty)),
                Assign(dest, NewArrayBounds(destElementType, sourceLength)),
                Call(ArrayCopyMethod, sourceExpression, dest, sourceLength),
                dest
            );
        }
    }
}
