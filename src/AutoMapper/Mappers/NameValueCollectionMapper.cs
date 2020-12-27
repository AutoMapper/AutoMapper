using System.Collections.Specialized;
using System.Linq.Expressions;
using AutoMapper.Internal;
namespace AutoMapper.Internal.Mappers
{
    public class NameValueCollectionMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType == context.DestinationType && context.SourceType.Name == nameof(NameValueCollection);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            Expression.New(typeof(NameValueCollection).GetConstructor(new[] { typeof(NameValueCollection) }), sourceExpression);
    }
}