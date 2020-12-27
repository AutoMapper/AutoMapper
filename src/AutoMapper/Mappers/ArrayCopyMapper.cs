using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    public class ArrayCopyMapper : ArrayMapper
    {
        private static readonly MethodInfo CopyToMethod = typeof(Array).GetMethod("CopyTo", new[] { typeof(Array), typeof(int) });
        public override TypePair GetAssociatedTypes(in TypePair context) => new TypePair(context.SourceType.GetElementType(), context.DestinationType.GetElementType());
        public override bool IsMatch(in TypePair context) => 
            context.SourceType == context.DestinationType && context.SourceType.IsArray && context.SourceType.GetElementType().IsPrimitive;
        public override Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destElementType = destExpression.Type.GetElementType();
            var sourceElementType = sourceExpression.Type.GetElementType();
            if (configurationProvider.FindTypeMapFor(sourceElementType, destElementType) != null)
            {
                return base.MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression);
            }
            var destination = Parameter(destExpression.Type, "destArray");
            return Block(
                new[] { destination },
                Assign(destination, NewArrayBounds(destElementType, ArrayLength(sourceExpression))),
                Call(sourceExpression, CopyToMethod, destination, ExpressionFactory.Zero),
                destination
            );
        }
    }
}