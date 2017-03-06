using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using Configuration;
    using static Expression;
    using static ExpressionExtensions;

    public class ArrayMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);

            var ifNullExpr = profileMap.AllowNullCollections
                                 ? (Expression) Constant(null, destExpression.Type)
                                 : NewArrayBounds(destElementType, Constant(0));

            ParameterExpression itemParam;
            var itemExpr = CollectionMapperExtensions.MapItemExpr(configurationProvider, profileMap, propertyMap, sourceExpression.Type, destExpression.Type, contextExpression, out itemParam);

            //var count = source.Count();
            //var array = new TDestination[count];

            //int i = 0;
            //foreach (var item in source)
            //    array[i++] = newItemFunc(item, context);
            //return array;

            var countParam = Parameter(typeof(int), "count");
            var arrayParam = Parameter(ifNullExpr.Type, "destinationArray");
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

            var mapExpr = Block(parameters, actions);

            // return (source == null) ? ifNullExpr : Map<TSourceElement, TDestElement>(source, context);
            return Condition(Equal(sourceExpression, Constant(null)), ifNullExpr, mapExpr);
        }
    }
}
