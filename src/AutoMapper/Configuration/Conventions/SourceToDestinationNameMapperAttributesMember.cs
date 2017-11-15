using System;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
    {
        private static readonly SourceMember[] Empty = new SourceMember[0];
        private LockingConcurrentDictionary<TypeDetails, SourceMember[]> _allSourceMembers
            = new LockingConcurrentDictionary<TypeDetails, SourceMember[]>(_ => Empty);

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            SourceMember[] ValueFactory(TypeDetails td) => getTypeInfoMembers.GetMemberInfos(td)
                .Select(sourceMember => new SourceMember(sourceMember))
                .Where(s => s.Attribute != null)
                .ToArray();

            var sourceMembers = _allSourceMembers.GetOrAdd(typeInfo, td => new Lazy<SourceMember[]>(() => ValueFactory(td)));

            return sourceMembers.FirstOrDefault(d => d.Attribute.IsMatch(typeInfo, d.Member, destType, destMemberType, nameToSearch)).Member;
        }

        struct SourceMember
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