using AutoMapper.Internal;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public abstract class EnumerableMapperBase : IObjectMapperInfo
    {
        public TypePair GetAssociatedTypes(in TypePair initialTypes)
        {
            var sourceElementType = ReflectionHelper.GetElementType(initialTypes.SourceType);
            var destElementType = ReflectionHelper.GetElementType(initialTypes.DestinationType);
            return new TypePair(sourceElementType, destElementType);
        }
        public abstract bool IsMatch(in TypePair context);
        public abstract Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression);
    }
}