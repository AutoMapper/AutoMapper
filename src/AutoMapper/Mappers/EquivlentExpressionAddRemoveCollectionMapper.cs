using System.Collections;
using System.Linq;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class EquivlentExpressionAddRemoveCollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var sourceElementType = TypeHelper.GetElementType(context.SourceValue.GetType());
            var destinationElementType = TypeHelper.GetElementType(context.DestinationValue.GetType());
            var equivilencyExpression = GetEquivilentExpression(context);

            var sourceEnumerable = context.SourceValue as IEnumerable;
            var destEnumerable = (IEnumerable) context.DestinationValue;

            var destItems = destEnumerable.Cast<object>().ToList();
            var sourceItems = sourceEnumerable.Cast<object>().ToList();
            var compareSourceToDestination = sourceItems.ToDictionary(s => s, s => destItems.FirstOrDefault(d => equivilencyExpression.IsEquivlent(s, d)));

            var actualDestType = destEnumerable.GetType();

            var addMethod = actualDestType.GetMethod("Add");
            foreach (var keypair in compareSourceToDestination)
            {
                if (keypair.Value == null)
                    addMethod.Invoke(destEnumerable, new[] {Mapper.Map(keypair.Key, sourceElementType, destinationElementType)});
                else
                    Mapper.Map(keypair.Key, keypair.Value, sourceElementType, destinationElementType);
            }

            var removeMethod = actualDestType.GetMethod("Remove");
            foreach (var removedItem in destItems.Except(compareSourceToDestination.Values))
                removeMethod.Invoke(destEnumerable, new[] { removedItem });

            return destEnumerable;
        }

        public bool IsMatch(ResolutionContext context)
        {
            return context.SourceValue != null && context.SourceValue.GetType().IsEnumerableType()
                && context.DestinationValue != null && context.DestinationValue.GetType().IsCollectionType() 
                && GetEquivilentExpression(context) != null;
        }

        private static IEquivilentExpression GetEquivilentExpression(ResolutionContext context)
        {
            return EquivilentExpressions.GetEquivilentExpression(TypeHelper.GetElementType(context.SourceType), TypeHelper.GetElementType(context.DestinationType));
        }
    }
}