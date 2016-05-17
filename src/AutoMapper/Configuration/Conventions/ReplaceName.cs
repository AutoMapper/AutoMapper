namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    public class ReplaceName : ISourceToDestinationNameMapper
    {
        private ICollection<MemberNameReplacer> MemberNameReplacers { get; }

        public ReplaceName()
        {
            MemberNameReplacers = new Collection<MemberNameReplacer>();
        }

        public ReplaceName AddReplace(string original, string newValue)
        {
            MemberNameReplacers.Add(new MemberNameReplacer(original, newValue));
            return this;
        }
        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            var possibleSourceNames = PossibleNames(nameToSearch);
            var possibleDestNames = getTypeInfoMembers.GetMemberInfos(typeInfo).Select(mi => new { mi, possibles = PossibleNames(mi.Name) });

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new { sourceName, destName };
            var match =
                all.FirstOrDefault(
                    pair => pair.destName.possibles.Any(p => string.Compare(p, pair.sourceName, StringComparison.OrdinalIgnoreCase) == 0));
            if (match == null)
                return null;
            return match.destName.mi;
        }

        private IEnumerable<string> PossibleNames(string nameToSearch)
        {
            return 
                MemberNameReplacers.Select(r => nameToSearch.Replace(r.OriginalValue, r.NewValue))
                    .Concat(new[] { MemberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OriginalValue, r.NewValue)), nameToSearch })
                    .ToList();
        }
    }
}