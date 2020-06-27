using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;
    using static CollectionMapperExpressionFactory;

    public class ArrayMapper : EnumerableMapperBase
    {
        public override bool IsMatch(TypePair context) => context.DestinationType.IsArray && context.SourceType.IsEnumerableType();

        public override Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceElementType = ElementTypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = ElementTypeHelper.GetElementType(destExpression.Type);

            var itemExpr = MapItemExpr(configurationProvider, profileMap, sourceExpression.Type, destExpression.Type, contextExpression, out ParameterExpression itemParam);

            //var count = source.Count();
            //var array = new TDestination[count];

            //int i = 0;
            //foreach (var item in source)
            //    array[i++] = newItemFunc(item, context);
            //return array;

            var countParam = Parameter(typeof(int), "count");
            var arrayParam = Parameter(destExpression.Type, "destinationArray");
            var indexParam = Parameter(typeof(int), "destinationArrayIndex");

            var actions = new List<Expression>();
            var parameters = new List<ParameterExpression> { countParam, arrayParam, indexParam };

            var countMethod = typeof(Enumerable)
                .GetTypeInfo()
                .DeclaredMethods
                .Single(mi => mi.Name == "Count" && mi.GetParameters().Length == 1)
                .MakeGenericMethod(sourceElementType);
            actions.Add(Assign(countParam, Call(countMethod, sourceExpression)));
            actions.Add(Assign(arrayParam, NewArrayBounds(destElementType, countParam)));
            actions.Add(Assign(indexParam, Constant(0)));
            actions.Add(ForEach(sourceExpression, itemParam,
                Assign(ArrayAccess(arrayParam, PostIncrementAssign(indexParam)), itemExpr)
                ));
            actions.Add(arrayParam);

            return Block(parameters, actions);
        }
    }
}
