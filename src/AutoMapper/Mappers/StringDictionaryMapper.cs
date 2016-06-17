using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;
using StringDictionary = System.Collections.Generic.IDictionary<string, object>;

namespace AutoMapper.Mappers
{
    using System;
    using Execution;

    public class ToStringDictionaryMapper : IObjectMapExpression
    {
        public static Dictionary<string, object> MembersDictionary(ResolutionContext context)
        {
            var source = context.SourceValue;
            var sourceTypeDetails = new TypeDetails(source.GetType(), _ => true, _ => true);
            var membersDictionary = sourceTypeDetails.PublicReadAccessors.ToDictionary(p => p.Name,
                p => p.GetMemberValue(source));
            return membersDictionary;
        }
        
        private static readonly MethodInfo MembersDictionaryMethodInfo = typeof(ToStringDictionaryMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return typeof(StringDictionary).IsAssignableFrom(context.DestinationType);
        }

        public object Map(ResolutionContext context)
            => context.MapCollection(null, typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyValuePairMethodInfo, sourceValue: MembersDictionary(context));

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, Call(MembersDictionaryMethodInfo, contextExpression), destExpression, contextExpression, _ => null, typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);
    }

    public class FromStringDictionaryMapper : IObjectMapExpression
    {
        public bool IsMatch(TypePair context)
        {
            return typeof(StringDictionary).IsAssignableFrom(context.SourceType);
        }

        private static TDestination Map<TDestination>(StringDictionary source, ResolutionContext context)
        {
            TDestination destination = context.Mapper.CreateObject<TDestination>(context);
            var destTypeDetails = new TypeDetails(context.DestinationType, _ => true, _ => true);
            var members = from name in source.Keys
                join member in destTypeDetails.PublicWriteAccessors on name equals member.Name
                select member;
            var memberContext = new ResolutionContext(context);
            foreach (var member in members)
            {
                object value = memberContext.MapMember(member, source[member.Name]);
                member.SetMemberValue(destination, value);
            }
            return destination;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(FromStringDictionaryMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.DestinationType).Invoke(null, new []{context.SourceValue, context});
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(destExpression.Type), sourceExpression, contextExpression);
        }
    }
}