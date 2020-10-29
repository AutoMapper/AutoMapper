using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class PrePostfixName : ISourceToDestinationNameMapper
    {
        public List<string> Prefixes { get; } = new List<string>();
        public List<string> Postfixes { get; } = new List<string>();
        public List<string> DestinationPrefixes { get; } = new List<string>();
        public List<string> DestinationPostfixes { get; } = new List<string>();
        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            MemberInfo member;
            foreach (var possibleSourceName in TypeDetails.PossibleNames(nameToSearch, DestinationPrefixes, DestinationPostfixes))
            {
                if ((member = sourceTypeDetails.GetMember(possibleSourceName)) != null)
                {
                    return member;
                }
            }
            return null;
        }
    }
}