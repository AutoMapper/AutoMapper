namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MapToAttribute : SourceToDestinationMapperAttribute
    {
        public string MatchingName { get; }

        public MapToAttribute(string matchingName)
        {
            MatchingName = matchingName;
        }

        public override bool IsMatch(TypeDetails typeInfo, MemberInfo memberInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            return string.Compare(MatchingName, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}