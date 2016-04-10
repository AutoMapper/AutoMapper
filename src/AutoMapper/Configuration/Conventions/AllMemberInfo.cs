namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class AllMemberInfo : IGetTypeInfoMembers
    {
        private readonly IList<Func<MemberInfo, bool>> _predicates = new List<Func<MemberInfo, bool>>();

        public IEnumerable<MemberInfo> GetMemberInfos(TypeDetails typeInfo)
        {
            return AllMembers(typeInfo).Where(m => _predicates.All(p => p(m))).ToList();
        }

        public IGetTypeInfoMembers AddCondition(Func<MemberInfo, bool> predicate)
        {
            _predicates.Add(predicate);
            return this;
        }

        private static IEnumerable<MemberInfo> AllMembers(TypeDetails typeInfo)
        {
            return typeInfo.PublicReadAccessors.Concat(typeInfo.PublicNoArgMethods).Concat(typeInfo.PublicNoArgExtensionMethods).ToList();
        }
    }
}