using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;
using AutoMapper.Internal;
using static System.Linq.Expressions.Expression;
using StringDictionary = System.Collections.Generic.IDictionary<string, object>;

namespace AutoMapper.Mappers
{
    public class FromStringDictionaryMapper : IObjectMapper
    {
        private static readonly MethodInfo MapDynamicMehod = typeof(FromStringDictionaryMapper).GetDeclaredMethod(nameof(MapDynamic)); 
        public bool IsMatch(TypePair context) => typeof(StringDictionary).IsAssignableFrom(context.SourceType);
        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, 
            Expression sourceExpression, Expression destExpression, Expression contextExpression) =>
                Call(MapDynamicMehod, sourceExpression, destExpression.ToObject(), Constant(destExpression.Type), contextExpression, Constant(profileMap));
        private static object MapDynamic(StringDictionary source, object boxedDestination, Type destinationType, ResolutionContext context, ProfileMap profileMap)
        {
            boxedDestination ??= DelegateFactory.CreateInstance(destinationType);
            int matchedCount = 0;
            foreach (var member in profileMap.CreateTypeDetails(destinationType).PublicWriteAccessors.Where(m => source.ContainsKey(m.Name)))
            {
                var value = context.MapMember(member, source[member.Name], boxedDestination);
                member.SetMemberValue(boxedDestination, value);
                matchedCount++;
            }
            if (matchedCount < source.Count)
            {
                MapInnerProperties();
            }
            return boxedDestination;
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
                            newDestination = DelegateFactory.CreateInstance(member.GetMemberType());
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