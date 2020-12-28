using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper.Internal.Mappers
{
    using Execution;
    using static Execution.ExpressionBuilder;
    using static Expression;
    using static ExpressionFactory;
    using static ReflectionHelper;
    public class CollectionMapper : IObjectMapperInfo
    {
        public TypePair GetAssociatedTypes(in TypePair context) => new TypePair(GetElementType(context.SourceType), GetEnumerableElementType(context.DestinationType));
        public bool IsMatch(in TypePair context) => context.IsCollection();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var destinationType = destExpression.Type;
            if (destinationType.IsGenericType(typeof(ReadOnlyCollection<>)))
            {
                return MapReadOnlyCollection(typeof(List<>), typeof(ReadOnlyCollection<>));
            }
            if (destinationType.IsGenericType(typeof(ReadOnlyDictionary<,>)) || destinationType.IsGenericType(typeof(IReadOnlyDictionary<,>)))
            {
                return MapReadOnlyCollection(typeof(Dictionary<,>), typeof(ReadOnlyDictionary<,>));
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
                MethodInfo addMethod;
                bool isIList;
                Type destinationCollectionType, destinationElementType;
                GetDestinationType();
                var passedDestination = Variable(destExpression.Type, "passedDestination");
                var newExpression = Variable(passedDestination.Type, "collectionDestination");
                var sourceElementType = sourceExpression.Type.GetICollectionType()?.GenericTypeArguments[0] ?? GetEnumerableElementType(sourceExpression.Type);
                var itemParam = Parameter(sourceElementType, "item");
                var itemExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, new TypePair(sourceElementType, destinationElementType), itemParam);
                Expression destination, assignNewExpression;
                UseDestinationValue();
                var addItems = ForEach(itemParam, sourceExpression, Call(destination, addMethod, itemExpr));
                var overMaxDepth = OverMaxDepth(memberMap?.TypeMap);
                if (overMaxDepth != null)
                {
                    addItems = Condition(overMaxDepth, ExpressionFactory.Empty, addItems);
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
                    if (destinationCollectionType == null && destinationType.IsInterface)
                    {
                        destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
                        destExpression = ToType(destExpression, destinationCollectionType);
                    }
                    if (destinationCollectionType == null)
                    {
                        destinationCollectionType = typeof(IList);
                        addMethod = IListAdd;
                        isIList = true;
                    }
                    else
                    {
                        isIList = destExpression.Type.IsListType();
                        addMethod = destinationCollectionType.GetMethod("Add");
                    }
                }
                void UseDestinationValue()
                {
                    if (memberMap is { UseDestinationValue: true })
                    {
                        destination = passedDestination;
                        assignNewExpression = ExpressionFactory.Empty;
                    }
                    else
                    {
                        destination = newExpression;
                        var createInstance = ObjectFactory.GenerateConstructorExpression(passedDestination.Type);
                        var shouldCreateDestination = ReferenceEqual(passedDestination, Null);
                        if (memberMap is { CanBeSet: true })
                        {
                            var isReadOnly = isIList ? Property(passedDestination, IListIsReadOnly) : ExpressionFactory.Property(ToType(passedDestination, destinationCollectionType), "IsReadOnly");
                            shouldCreateDestination = OrElse(shouldCreateDestination, isReadOnly);
                        }
                        assignNewExpression = Assign(newExpression, Condition(shouldCreateDestination, ToType(createInstance, passedDestination.Type), passedDestination));
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
    }
}