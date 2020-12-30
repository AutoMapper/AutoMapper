using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
namespace AutoMapper.Internal.Mappers
{
    using Execution;
    using static Expression;
    using static ReflectionHelper;
    using static Execution.ExpressionBuilder;
    public class ArrayMapper : IObjectMapperInfo
    {
        private static readonly MethodInfo CopyToMethod = typeof(Array).GetMethod("CopyTo", new[] { typeof(Array), typeof(int) });
        private static readonly MethodInfo CountMethod = typeof(Enumerable).StaticGenericMethod("Count", parametersCount: 1);
        private static readonly MethodInfo MapMultidimensionalMethod = typeof(ArrayMapper).GetStaticMethod(nameof(MapMultidimensional));
        public bool IsMatch(in TypePair context) => context.DestinationType.IsArray && context.SourceType.IsCollection();
        public TypePair GetAssociatedTypes(in TypePair context) => new TypePair(GetElementType(context.SourceType), context.DestinationType.GetElementType());
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationType = destExpression.Type;
            var destinationElementType = destinationType.GetElementType();
            if (destinationType.GetArrayRank() > 1)
            {
                return Call(MapMultidimensionalMethod, sourceExpression, Constant(destinationElementType), ContextParameter);
            }
            var sourceType = sourceExpression.Type;
            Type sourceElementType;
            Expression createDestination;
            var destination = Parameter(destinationType, "destinationArray");
            if (sourceType.IsArray)
            {
                sourceElementType = sourceType.GetElementType();
                createDestination = Assign(destination, NewArrayBounds(destinationElementType, ArrayLength(sourceExpression)));
                if (destinationElementType.IsAssignableFrom(sourceElementType) && configurationProvider.FindTypeMapFor(sourceElementType, destinationElementType) == null)
                {
                    return Block(new[] { destination },
                        createDestination,
                        Call(sourceExpression, CopyToMethod, destination, Zero),
                        destination);
                }
            }
            else
            {
                sourceElementType = GetEnumerableElementType(sourceExpression.Type);
                var count = Call(CountMethod.MakeGenericMethod(sourceElementType), sourceExpression);
                createDestination = Assign(destination, NewArrayBounds(destinationElementType, count));
            }
            var itemParam = Parameter(sourceElementType, "sourceItem");
            var itemExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, new TypePair(sourceElementType, destinationElementType), itemParam);
            var indexParam = Parameter(typeof(int), "destinationArrayIndex");
            var setItem = Assign(ArrayAccess(destination, PostIncrementAssign(indexParam)), itemExpr);
            return Block(new[] { destination, indexParam },
                createDestination,
                Assign(indexParam, Zero),
                ForEach(itemParam, sourceExpression, setItem),
                destination);
        }
        private static Array MapMultidimensional(Array source, Type destinationElementType, ResolutionContext context)
        {
            var sourceElementType = source.GetType().GetElementType();
            var destinationArray = Array.CreateInstance(destinationElementType, Enumerable.Range(0, source.Rank).Select(source.GetLength).ToArray());
            var filler = new MultidimensionalArrayFiller(destinationArray);
            foreach (var item in source)
            {
                filler.NewValue(context.Map(item, null, sourceElementType, destinationElementType, null));
            }
            return destinationArray;
        }
    }
    public class MultidimensionalArrayFiller
    {
        private readonly int[] _indices;
        private readonly Array _destination;
        public MultidimensionalArrayFiller(Array destination)
        {
            _indices = new int[destination.Rank];
            _destination = destination;
        }
        public void NewValue(object value)
        {
            var dimension = _destination.Rank - 1;
            var changedDimension = false;
            while (_indices[dimension] == _destination.GetLength(dimension))
            {
                _indices[dimension] = 0;
                dimension--;
                if (dimension < 0)
                {
                    throw new InvalidOperationException("Not enough room in destination array " + _destination);
                }
                _indices[dimension]++;
                changedDimension = true;
            }
            _destination.SetValue(value, _indices);
            if (changedDimension)
            {
                _indices[dimension + 1]++;
            }
            else
            {
                _indices[dimension]++;
            }
        }
    }
}