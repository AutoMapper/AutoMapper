using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using AutoMapper.Internal;
namespace AutoMapper.Mappers
{
    using static ExpressionFactory;
    public class ReadOnlyDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsCollection() && IsReadOnlyDictionaryType(context.DestinationType);
        public static bool IsReadOnlyDictionaryType(Type type) => type.IsGenericType(typeof(IReadOnlyDictionary<,>)) || type.IsGenericType(typeof(ReadOnlyDictionary<,>));
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            MapReadOnlyCollection(typeof(Dictionary<,>), typeof(ReadOnlyDictionary<,>), configurationProvider, profileMap, memberMap, sourceExpression, destExpression);
    }
}