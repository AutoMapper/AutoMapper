using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;
using StringDictionary = System.Collections.Generic.IDictionary<string, object>;
namespace AutoMapper.Internal.Mappers
{
    using static Expression;
    using static ExpressionBuilder;
    public class FromStringDictionaryMapper : IObjectMapper
    {
        private static readonly MethodInfo MapDynamicMethod = typeof(FromStringDictionaryMapper).GetStaticMethod(nameof(MapDynamic));
        public bool IsMatch(in TypePair context) => typeof(StringDictionary).IsAssignableFrom(context.SourceType);
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap,
            Expression sourceExpression, Expression destExpression) =>
                Call(MapDynamicMethod, sourceExpression, destExpression.ToObject(), Constant(destExpression.Type), ContextParameter, Constant(profileMap));
        struct Match
        {
            public object Value;
            public int Count;
        }
        private static object MapDynamic(StringDictionary source, object boxedDestination, Type destinationType, ResolutionContext context, ProfileMap profileMap)
        {
            boxedDestination ??= ObjectFactory.CreateInstance(destinationType);
            int matchedCount = 0;
            foreach (var member in profileMap.CreateTypeDetails(destinationType).WriteAccessors)
            {
                var match = MatchSource(member.Name);
                if (match.Count == 0)
                {
                    continue;
                }
                if (match.Count > 1)
                {
                    throw new AutoMapperMappingException($"Multiple matching keys were found in the source dictionary for destination member {member}.", null, new TypePair(typeof(StringDictionary), destinationType));
                }
                var value = context.MapMember(member, match.Value, boxedDestination);
                member.SetMemberValue(boxedDestination, value);
                matchedCount++;
            }
            if (matchedCount < source.Count)
            {
                MapInnerProperties();
            }
            return boxedDestination;
            Match MatchSource(string name)
            {
                if (source.TryGetValue(name, out var value))
                {
                    return new Match { Value = value, Count = 1 };
                }
                var matches = source.Where(s => s.Key.Trim() == name).Select(s=>s.Value).ToArray();
                if (matches.Length == 1)
                {
                    return new Match { Value = matches[0], Count = 1 };
                }
                return new Match { Count = matches.Length };
            }
            void MapInnerProperties()
            {
                MemberInfo[] innerMembers;
                foreach (var memberPath in source.Keys.Where(k => k.Contains('.')))
                {
                    innerMembers = ReflectionHelper.GetMemberPath(destinationType, memberPath);
                    var innerDestination = GetInnerDestination();
                    if (innerDestination == null)
                    {
                        continue;
                    }
                    var lastMember = innerMembers[innerMembers.Length - 1];
                    var value = context.MapMember(lastMember, source[memberPath], innerDestination);
                    lastMember.SetMemberValue(innerDestination, value);
                }
                return;
                object GetInnerDestination()
                {
                    var currentDestination = boxedDestination;
                    foreach (var member in innerMembers.Take(innerMembers.Length - 1))
                    {
                        var newDestination = member.GetMemberValue(currentDestination);
                        if (newDestination == null)
                        {
                            if (!member.CanBeSet())
                            {
                                return null;
                            }
                            newDestination = ObjectFactory.CreateInstance(member.GetMemberType());
                            member.SetMemberValue(currentDestination, newDestination);
                        }
                        currentDestination = newDestination;
                    }
                    return currentDestination;
                }
            }
        }
    }
}