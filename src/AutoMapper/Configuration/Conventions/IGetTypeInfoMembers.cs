namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IGetTypeInfoMembers
    {
        IEnumerable<MemberInfo> GetMemberInfos(TypeDetails typeInfo);
        IGetTypeInfoMembers AddCondition(Func<MemberInfo, bool> predicate);
    }
}