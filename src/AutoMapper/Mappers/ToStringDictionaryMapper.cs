﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static CollectionMapperExpressionFactory;

    public class ToStringDictionaryMapper : IObjectMapper
    {
        private static readonly MethodInfo MembersDictionaryMethodInfo =
            typeof(ToStringDictionaryMapper).GetDeclaredMethod(nameof(MembersDictionary));

        public bool IsMatch(in TypePair context) => typeof(IDictionary<string, object>).IsAssignableFrom(context.DestinationType);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => MapCollectionExpression(configurationProvider, profileMap, propertyMap,
                Call(MembersDictionaryMethodInfo, sourceExpression, Constant(profileMap)), destExpression, contextExpression, typeof(Dictionary<,>),
                MapKeyPairValueExpr);

        private static Dictionary<string, object> MembersDictionary(object source, ProfileMap profileMap)
        {
            var sourceTypeDetails = profileMap.CreateTypeDetails(source.GetType());
            var membersDictionary = sourceTypeDetails.PublicReadAccessors.ToDictionary(p => p.Name, p => p.GetMemberValue(source));
            return membersDictionary;
        }
    }
}