using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class PrePostfixName : ISourceToDestinationNameMapper
    {
        private readonly List<string> _prefixes = new List<string>();
        private readonly List<string> _postfixes = new List<string>();
        private readonly List<string> _destinationPrefixes = new List<string>();
        private readonly List<string> _destinationPostfixes = new List<string>();

        public ICollection<string> Prefixes => _prefixes;
        public ICollection<string> Postfixes => _postfixes;
        public ICollection<string> DestinationPrefixes => _destinationPrefixes;
        public ICollection<string> DestinationPostfixes => _destinationPostfixes;
        
        public PrePostfixName AddStrings(Func<PrePostfixName, ICollection<string>> getStringsFunc, params string[] names)
        {
            var strings = getStringsFunc(this);
            foreach (var name in names)
            {
                strings.Add(name);
            }
            return this;
        }

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            var member = typeInfo.GetMember(nameToSearch);
            if (member != null)
            {
                return member;
            }
            foreach (var possibleSourceName in TypeDetails.PossibleNames(nameToSearch, _destinationPrefixes, _destinationPostfixes))
            {
                if ((member = typeInfo.GetMember(possibleSourceName)) != null)
                {
                    return member;
                }
            }
            return null;
        }
    }
}