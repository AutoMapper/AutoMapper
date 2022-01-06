using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;
namespace AutoMapper.Internal.Mappers
{
    using Execution;
    using static Execution.ExpressionBuilder;
    using static Expression;
    using static ReflectionHelper;
    public class CollectionMapper : IObjectMapperInfo
    {
        public TypePair GetAssociatedTypes(TypePair context) => new(GetElementType(context.SourceType), GetElementType(context.DestinationType));
        public bool IsMatch(TypePair context) => context.SourceType.IsCollection() && context.DestinationType.IsCollection();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationType = destExpression.Type;
            if (destinationType.IsArray)
            {
                return ArrayMapper.MapToArray(configurationProvider, profileMap, sourceExpression, destinationType);
            }
            if (destinationType.IsGenericType(typeof(ReadOnlyCollection<>)))
            {
                return MapReadOnlyCollection(typeof(List<>), typeof(ReadOnlyCollection<>));
            }
            if (destinationType.IsGenericType(typeof(ReadOnlyDictionary<,>)) || destinationType.IsGenericType(typeof(IReadOnlyDictionary<,>)))
            {
                return MapReadOnlyCollection(typeof(Dictionary<,>), typeof(ReadOnlyDictionary<,>));
            }
            if (destinationType == sourceExpression.Type && destinationType.Name == nameof(NameValueCollection))
            {
                return CreateNameValueCollection(sourceExpression);
            }
            return MapCollectionCore(destExpression);
            Expression MapReadOnlyCollection(Type genericCollectionType, Type genericReadOnlyCollectionType)
            {
                var destinationTypeArguments = destinationType.GenericTypeArguments;
                var closedCollectionType = genericCollectionType.MakeGenericType(destinationTypeArguments);
                var dict = MapCollectionCore(Default(closedCollectionType));
                var readOnlyClosedType = destinationType.IsInterface ? genericReadOnlyCollectionType.MakeGenericType(destinationTypeArguments) : destinationType;
                return New(readOnlyClosedType.GetConstructors()[0], dict);
            }
            Expression MapCollectionCore(Expression destExpression)
            {
                var destinationType = destExpression.Type;
                var sourceType = sourceExpression.Type;
                MethodInfo addMethod;
                bool isIList, mustUseDestination = memberMap is { MustUseDestination: true };
                Type destinationCollectionType, destinationElementType;
                GetDestinationType();
                var passedDestination = Variable(destExpression.Type, "passedDestination");
                var newExpression = Variable(passedDestination.Type, "collectionDestination");
                var sourceElementType = sourceType.GetICollectionType()?.GenericTypeArguments[0] ?? GetEnumerableElementType(sourceType);
                if (sourceType == sourceElementType && destinationType == destinationElementType)
                {
                    throw new NotSupportedException($"Recursive collection. Create a custom type converter from {sourceType} to {destinationType}.");
                }
                var itemParam = Parameter(sourceElementType, "item");
                var itemExpr = configurationProvider.MapExpression(profileMap, new TypePair(sourceElementType, destinationElementType), itemParam);
                Expression destination, assignNewExpression;
                UseDestinationValue();
                var addItems = ForEach(itemParam, sourceExpression, Call(destination, addMethod, itemExpr));
                var overMaxDepth = OverMaxDepth(memberMap?.TypeMap);
                if (overMaxDepth != null)
                {
                    addItems = Condition(overMaxDepth, ExpressionBuilder.Empty, addItems);
                }
                var clearMethod = isIList ? IListClear : destinationCollectionType.GetMethod("Clear");
                var checkNull = Block(new[] { newExpression, passedDestination },
                        Assign(passedDestination, destExpression),
                        assignNewExpression,
                        Call(destination, clearMethod),
                        addItems,
                        destination);
                if (memberMap != null)
                {
                    return checkNull;
                }
                return CheckContext();
                void GetDestinationType()
                {
                    destinationCollectionType = destinationType.GetICollectionType();
                    destinationElementType = destinationCollectionType?.GenericTypeArguments[0] ?? GetEnumerableElementType(destinationType);
                    isIList = destExpression.Type.IsListType();
                    if (destinationCollectionType == null)
                    {
                        if (isIList)
                        {
                            destinationCollectionType = typeof(IList);
                            addMethod = IListAdd;
                        }
                        else
                        {
                            destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
                            destExpression = Convert(mustUseDestination ? destExpression : Null, destinationCollectionType);
                            addMethod = destinationCollectionType.GetMethod("Add");
                        }
                    }
                    else
                    {
                        addMethod = destinationCollectionType.GetMethod("Add");
                    }
                }
                void UseDestinationValue()
                {
                    if (mustUseDestination)
                    {
                        destination = passedDestination;
                        assignNewExpression = ExpressionBuilder.Empty;
                    }
                    else
                    {
                        destination = newExpression;
                        assignNewExpression = Assign(newExpression, Coalesce(passedDestination, ObjectFactory.GenerateConstructorExpression(passedDestination.Type)));
                    }
                }
                Expression CheckContext()
                {
                    var elementTypeMap = configurationProvider.ResolveTypeMap(sourceElementType, destinationElementType);
                    if (elementTypeMap == null)
                    {
                        return checkNull;
                    }
                    var checkContext = ExpressionBuilder.CheckContext(elementTypeMap);
                    if (checkContext == null)
                    {
                        return checkNull;
                    }
                    return Block(checkContext, checkNull);
                }
            }
        }
        private static Expression CreateNameValueCollection(Expression sourceExpression) =>
            New(typeof(NameValueCollection).GetConstructor(new[] { typeof(NameValueCollection) }), sourceExpression);
        static class ArrayMapper
        {
            private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetStaticMethod("ToArray");
            private static readonly MethodInfo CopyToMethod = typeof(Array).GetMethod("CopyTo", new[] { typeof(Array), typeof(int) });
            private static readonly MethodInfo CountMethod = typeof(Enumerable).StaticGenericMethod("Count", parametersCount: 1);
            private static readonly MethodInfo MapMultidimensionalMethod = typeof(ArrayMapper).GetStaticMethod(nameof(MapMultidimensional));
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
            public static Expression MapToArray(IGlobalConfiguration configurationProvider, ProfileMap profileMap, Expression sourceExpression, Type destinationType)
            {
                var destinationElementType = destinationType.GetElementType();
                if (destinationType.GetArrayRank() > 1)
                {
                    return Call(MapMultidimensionalMethod, sourceExpression, Constant(destinationElementType), ContextParameter);
                }
                var sourceType = sourceExpression.Type;
                Type sourceElementType = typeof(object);
                Expression createDestination;
                var destination = Parameter(destinationType, "destinationArray");
                if (sourceType.IsArray)
                {
                    var mapFromArray = MapFromArray();
                    if (mapFromArray != null)
                    {
                        return mapFromArray;
                    }
                }
                else
                {
                    var mapFromIEnumerable = MapFromIEnumerable();
                    if (mapFromIEnumerable != null)
                    {
                        return mapFromIEnumerable;
                    }
                    var count = Call(CountMethod.MakeGenericMethod(sourceElementType), sourceExpression);
                    createDestination = Assign(destination, NewArrayBounds(destinationElementType, count));
                }
                var itemParam = Parameter(sourceElementType, "sourceItem");
                var itemExpr = configurationProvider.MapExpression(profileMap, new TypePair(sourceElementType, destinationElementType), itemParam);
                var indexParam = Parameter(typeof(int), "destinationArrayIndex");
                var setItem = Assign(ArrayAccess(destination, PostIncrementAssign(indexParam)), itemExpr);
                return Block(new[] { destination, indexParam },
                    createDestination,
                    Assign(indexParam, Zero),
                    ForEach(itemParam, sourceExpression, setItem),
                    destination);
                Expression MapFromArray()
                {
                    sourceElementType = sourceType.GetElementType();
                    createDestination = Assign(destination, NewArrayBounds(destinationElementType, ArrayLength(sourceExpression)));
                    if (!destinationElementType.IsAssignableFrom(sourceElementType) || 
                        configurationProvider.FindTypeMapFor(sourceElementType, destinationElementType) != null)
                    {
                        return null;
                    }
                    return Block(new[] { destination },
                        createDestination,
                        Call(sourceExpression, CopyToMethod, destination, Zero),
                        destination);
                }
                Expression MapFromIEnumerable()
                {
                    var iEnumerableType = sourceType.GetIEnumerableType();
                    if (iEnumerableType == null || (sourceElementType = iEnumerableType.GenericTypeArguments[0]) != destinationElementType ||
                        configurationProvider.FindTypeMapFor(sourceElementType, destinationElementType) != null)
                    {
                        return null;
                    }
                    return Call(ToArrayMethod.MakeGenericMethod(sourceElementType), sourceExpression);
                }
            }
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