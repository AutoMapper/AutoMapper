using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class ReplaceName : ISourceToDestinationNameMapper
    {
        private readonly List<MemberNameReplacer> _memberNameReplacers = new List<MemberNameReplacer>();

        public ReplaceName AddReplace(string original, string newValue)
        {
            _memberNameReplacers.Add(new MemberNameReplacer(original, newValue));
            return this;
        }
        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            var possibleSourceNames = PossibleNames(nameToSearch);
            var possibleDestNames = sourceTypeDetails.PublicReadAccessors.Select(mi => new { mi, possibles = PossibleNames(mi.Name) });

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new { sourceName, destName };
            var match =
                all.FirstOrDefault(
                    pair => pair.destName.possibles.Any(p => string.Compare(p, pair.sourceName, StringComparison.OrdinalIgnoreCase) == 0));

            return match?.destName.mi;
        }

        private IEnumerable<string> PossibleNames(string nameToSearch)
        {
            return 
                _memberNameReplacers.Select(r => nameToSearch.Replace(r.OriginalValue, r.NewValue))
                    .Concat(new[] { _memberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OriginalValue, r.NewValue)), nameToSearch })
                    .ToList();
        }
    }
}