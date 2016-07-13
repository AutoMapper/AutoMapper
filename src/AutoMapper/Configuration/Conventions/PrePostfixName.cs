namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    public class PrePostfixName : ISourceToDestinationNameMapper
    {
        public ICollection<string> Prefixes { get; } = new Collection<string>();
        public ICollection<string> Postfixes { get; } = new Collection<string>();
        public ICollection<string> DestinationPrefixes { get; } = new Collection<string>();
        public ICollection<string> DestinationPostfixes { get; } = new Collection<string>();

        public PrePostfixName AddStrings(Func<PrePostfixName, ICollection<string>> getStringsFunc, params string[] names)
        {
            var strings = getStringsFunc(this);
            foreach (var name in names)
                strings.Add(name);
            return this;
        }

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            var possibleSourceNames = DestinationPostfixes.Any() || DestinationPrefixes.Any()
                ? PossibleNames(nameToSearch, DestinationPrefixes, DestinationPostfixes)
                : new[] {nameToSearch};

            var all =
                from sourceName in possibleSourceNames
                from destName in typeInfo.DestinationMemberNames
                select new { sourceName, destName };
            var match =
                all.FirstOrDefault(
                    pair => pair.destName.Possibles.Any(p => string.Compare(p, pair.sourceName, StringComparison.OrdinalIgnoreCase) == 0));
            return match?.destName.Member;
        }

        private IEnumerable<string> PossibleNames(string memberName, IEnumerable<string> prefixes, IEnumerable<string> postfixes)
        {
            if (string.IsNullOrEmpty(memberName))
                yield break;

            yield return memberName;

            foreach (var withoutPrefix in prefixes.Where(prefix => memberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).Select(prefix => memberName.Substring(prefix.Length)))
            {
                yield return withoutPrefix;
                foreach (var s in PostFixes(postfixes, withoutPrefix))
                    yield return s;
            }
            foreach (var s in PostFixes(postfixes, memberName))
                yield return s;
        }

        private IEnumerable<string> PostFixes(IEnumerable<string> postfixes, string name)
        {
            return
                postfixes.Where(postfix => name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
                    .Select(postfix => name.Remove(name.Length - postfix.Length));
        }
    }
}