using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    using static ExpressionFactory;
    public class ToStringDictionaryMapper : IObjectMapper
    {
        private static readonly MethodInfo MembersDictionaryMethodInfo = typeof(ToStringDictionaryMapper).GetStaticMethod(nameof(MembersDictionary));
        public bool IsMatch(in TypePair context) => typeof(IDictionary<string, object>).IsAssignableFrom(context.DestinationType);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
            => MapCollectionExpression(configurationProvider, profileMap, memberMap,
                Call(MembersDictionaryMethodInfo, sourceExpression, Constant(profileMap)), destExpression);
        private static Dictionary<string, object> MembersDictionary(object source, ProfileMap profileMap)
        {
            var sourceTypeDetails = profileMap.CreateTypeDetails(source.GetType());
            var membersDictionary = sourceTypeDetails.ReadAccessors.ToDictionary(p => p.Name, p => p.GetMemberValue(source));
            return membersDictionary;
        }
    }
}