using AutoMapper.Internal;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public abstract class EnumerableMapperBase : IObjectMapperInfo
    {
        public TypePair GetAssociatedTypes(TypePair initialTypes)
        {
            var sourceElementType = ElementTypeHelper.GetElementType(initialTypes.SourceType);
            var destElementType = ElementTypeHelper.GetElementType(initialTypes.DestinationType);
            return new TypePair(sourceElementType, destElementType);
        }

        public abstract bool IsMatch(TypePair context);
        public abstract Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression);
    }
}