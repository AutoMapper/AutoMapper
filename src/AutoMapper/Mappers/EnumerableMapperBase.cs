using System.Linq.Expressions;
using AutoMapper.Mappers.Internal;

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
    }
}