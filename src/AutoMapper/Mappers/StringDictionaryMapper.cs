using static System.Linq.Expressions.Expression;
using StringDictionary = System.Collections.Generic.IDictionary<string, object>;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

    public class ToStringDictionaryMapper : IObjectMapper
    {
        private static readonly MethodInfo MembersDictionaryMethodInfo =
            typeof(ToStringDictionaryMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return typeof(StringDictionary).IsAssignableFrom(context.DestinationType);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap,
            Expression sourceExpression, Expression destExpression, Expression contextExpression)
            =>
            configurationProvider.MapCollectionExpression(propertyMap,
                Call(MembersDictionaryMethodInfo, sourceExpression, contextExpression), destExpression, contextExpression, _ => null,
                typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);

        public static Dictionary<string, object> MembersDictionary(object source, ResolutionContext context)
        {
            var sourceTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(source.GetType());
            var membersDictionary = sourceTypeDetails.PublicReadAccessors.ToDictionary(p => p.Name,
                p => p.GetMemberValue(source));
            return membersDictionary;
        }
    }

    public class FromStringDictionaryMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo =
            typeof(FromStringDictionaryMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return typeof(StringDictionary).IsAssignableFrom(context.SourceType);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap,
            Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Call(null, MapMethodInfo.MakeGenericMethod(destExpression.Type), sourceExpression, destExpression, contextExpression);
        }

        private static TDestination Map<TDestination>(StringDictionary source, TDestination destination, ResolutionContext context)
        {
            destination = destination == null ? context.Mapper.CreateObject<TDestination>() : destination;
            var destTypeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TDestination));
            var members = from name in source.Keys
                          join member in destTypeDetails.PublicWriteAccessors on name equals member.Name
                          select member;
            object boxedDestination = destination;
            foreach (var member in members)
            {
                var value = context.MapMember(member, source[member.Name], boxedDestination);
                member.SetMemberValue(boxedDestination, value);
            }
            return (TDestination) boxedDestination;
        }
    }
}