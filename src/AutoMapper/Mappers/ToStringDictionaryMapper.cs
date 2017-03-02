using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public class ToStringDictionaryMapper : IObjectMapper
    {
        private static readonly MethodInfo MembersDictionaryMethodInfo =
            typeof(ToStringDictionaryMapper).GetDeclaredMethod(nameof(MembersDictionary));

        public bool IsMatch(TypePair context)
        {
            return typeof(IDictionary<string, object>).IsAssignableFrom(context.DestinationType);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap,
            Expression sourceExpression, Expression destExpression, Expression contextExpression)
            =>
                configurationProvider.MapCollectionExpression(propertyMap,
                    Expression.Call(MembersDictionaryMethodInfo, sourceExpression, contextExpression), destExpression, contextExpression, _ => null,
                    typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);

        private static Dictionary<string, object> MembersDictionary(object source, ResolutionContext context)
        {
            var sourceTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(source.GetType());
            var membersDictionary = sourceTypeDetails.PublicReadAccessors.ToDictionary(p => p.Name, p => p.GetMemberValue(source));
            return membersDictionary;
        }
    }
}