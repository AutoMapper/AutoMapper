using System;
using System.Collections;
using System.Linq;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class EquivlentExpressionAddRemoveCollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var sourceElementType = TypeHelper.GetElementType(context.SourceType);
            var destinationElementType = TypeHelper.GetElementType(context.DestinationType);
            var equivilencyExpression = GetEquivilentExpression(context);

            var sourceEnumerable = context.SourceValue as IEnumerable;
            var destinationCollection = (IEnumerable) context.DestinationValue;
            var destItems = destinationCollection.Cast<object>().ToList();
            var comparisionDictionary = sourceEnumerable.Cast<object>()
                .ToDictionary(s => s, s => destItems.FirstOrDefault(d => equivilencyExpression.IsEquivlent(s, d)));

            foreach (var keypair in comparisionDictionary)
            {
                if (keypair.Value == null)
                    Add(destinationCollection, destinationElementType, Mapper.Map(keypair.Key, sourceElementType, destinationElementType));
                else
                    Mapper.Map(keypair.Key, keypair.Value, sourceElementType, destinationElementType);
            }
            foreach (var removedItem in destItems.Except(comparisionDictionary.Values))
                Remove(destinationCollection, destinationElementType, removedItem);

            return destinationCollection;
        }

        private void Add(IEnumerable enumerable, Type type, object item)
        {
            enumerable.GetType().GetMethod("Add").Invoke(enumerable, new [] {item});
        }

        private void Remove(IEnumerable enumerable, Type type, object item)
        {
            enumerable.GetType().GetMethod("Remove").Invoke(enumerable, new[] { item });
        }

        public bool IsMatch(ResolutionContext context)
        {
            return context.SourceType.IsEnumerableType() && context.SourceValue != null
                && context.DestinationType.IsCollectionType() && context.DestinationValue != null
                && GetEquivilentExpression(context) != null;
        }

        private static IEquivilentExpression GetEquivilentExpression(ResolutionContext context)
        {
            return EquivilentExpressions.GetEquivilentExpression(TypeHelper.GetElementType(context.SourceType), TypeHelper.GetElementType(context.DestinationType));
        }
    }
}