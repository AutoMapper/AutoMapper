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
            var possibleDestNames = sourceTypeDetails.ReadAccessors.Select(mi => new { mi, possibles = PossibleNames(mi.Name) }).ToArray();
            foreach (var sourceName in possibleSourceNames)
            {
                foreach (var destName in possibleDestNames)
                {
                    if (Array.Exists(destName.possibles, name => string.Compare(name, sourceName, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return destName.mi;
                    }
                }
            }
            return null;
        }
        private string[] PossibleNames(string nameToSearch) =>
                _memberNameReplacers.Select(r => nameToSearch.Replace(r.OriginalValue, r.NewValue))
                    .Concat(new[] { _memberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OriginalValue, r.NewValue)), nameToSearch })
                    .ToArray();
    }
}