namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
    {
        private static readonly ConcurrentDictionary<TypeDetails, Dictionary<MemberInfo, IEnumerable<SourceToDestinationMapperAttribute>>> Cache = new ConcurrentDictionary<TypeDetails, Dictionary<MemberInfo, IEnumerable<SourceToDestinationMapperAttribute>>>();

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, string nameToSearch)
        {
            Cache.GetOrAdd(typeInfo, ti => getTypeInfoMembers.GetMemberInfos(ti).ToDictionary(mi => mi, mi => CustomAttributeExtensions.GetCustomAttributes((MemberInfo) mi, typeof(SourceToDestinationMapperAttribute), true).OfType<SourceToDestinationMapperAttribute>()));

            return Cache[typeInfo].FirstOrDefault(kp => kp.Value.Any(_ => _.IsMatch(typeInfo, kp.Key, destType, nameToSearch))).Key;
        }
    }
}