namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;

    public interface IMemberConfiguration
    {
        IList<IChildMemberConfiguration> MemberMappers { get; }
        IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new();

        IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
            where TNameMapper : ISourceToDestinationNameMapper, new();

        IParentSourceToDestinationNameMapper NameMapper { get; set; }
        bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<IMemberGetter> resolvers);
    }
}