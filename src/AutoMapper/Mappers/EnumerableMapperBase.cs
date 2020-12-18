using AutoMapper.Internal;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    public abstract class EnumerableMapperBase : IObjectMapperInfo
    {
        public abstract TypePair GetAssociatedTypes(in TypePair context);
        public abstract bool IsMatch(in TypePair context);
        public abstract Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression);
    }
}