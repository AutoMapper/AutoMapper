using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;
namespace AutoMapper.Mappers
{
    using Execution;
    using static Expression;
    using static ExpressionFactory;
    using static ReflectionHelper;
    public class ArrayMapper : EnumerableMapperBase
    {
        public override TypePair GetAssociatedTypes(in TypePair context) => 
            new TypePair(GetElementType(context.SourceType), context.DestinationType.GetElementType());
        public override bool IsMatch(in TypePair context) => context.DestinationType.IsArray && context.SourceType.IsCollection();
        public override Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var sourceType = sourceExpression.Type;
            Type sourceElementType;
            Expression count;
            if (sourceType.IsArray)
            {
                sourceElementType = sourceType.GetElementType();
                count = ArrayLength(sourceExpression);
            }
            else
            {
                sourceElementType = GetEnumerableElementType(sourceExpression.Type);
                count = ExpressionFactory.Call(typeof(Enumerable), "Count", new[] { sourceElementType }, sourceExpression);
            }
            var destinationElementType = destExpression.Type.GetElementType();
            var itemParam = Parameter(sourceElementType, "sourceItem");
            var itemExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, new TypePair(sourceElementType, destinationElementType), itemParam);
            //var array = new TDestination[source.Count()];
            //int i = 0;
            //foreach (var item in source)
            //    array[i++] = Map(item, context);
            //return array;
            var arrayParam = Parameter(destExpression.Type, "destinationArray");
            var indexParam = Parameter(typeof(int), "destinationArrayIndex");
            var setItem = Assign(ArrayAccess(arrayParam, PostIncrementAssign(indexParam)), itemExpr);
            return Block(new[] { arrayParam, indexParam },
                Assign(arrayParam, NewArrayBounds(destinationElementType, count)),
                Assign(indexParam, Zero),
                ForEach(itemParam, sourceExpression, setItem),
                arrayParam);
        }
    }
}