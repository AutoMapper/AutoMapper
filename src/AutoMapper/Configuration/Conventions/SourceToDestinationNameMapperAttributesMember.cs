using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
    {
        private static readonly SourceMember[] Empty = new SourceMember[0];
        private readonly Dictionary<TypeDetails, SourceMember[]> _allSourceMembers = new Dictionary<TypeDetails, SourceMember[]>();

        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            if (!_allSourceMembers.TryGetValue(sourceTypeDetails, out SourceMember[] sourceMembers))
            {
                sourceMembers = sourceTypeDetails.PublicReadAccessors.Select(sourceMember => new SourceMember(sourceMember)).Where(s => s.Attribute != null).ToArray();
                _allSourceMembers[sourceTypeDetails] = sourceMembers.Length == 0 ? Empty : sourceMembers;
            }
            return sourceMembers.FirstOrDefault(d => d.Attribute.IsMatch(sourceTypeDetails, d.Member, destType, destMemberType, nameToSearch)).Member;
        }

        readonly struct SourceMember
        {
            public SourceMember(MemberInfo sourceMember)
            {
                Member = sourceMember;
                Attribute = sourceMember.GetCustomAttribute<SourceToDestinationMapperAttribute>(inherit:true);
            }

            public MemberInfo Member { get; }
            public SourceToDestinationMapperAttribute Attribute { get; }
        }
    }
}