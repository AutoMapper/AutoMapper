using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace AutoMapper.Configuration.Conventions
{
    public interface ISourceToDestinationNameMapper
    {
        MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch);
    }
    public interface IParentSourceToDestinationNameMapper
    {
        List<ISourceToDestinationNameMapper> NamedMappers { get; }
        MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch);
    }
    public sealed class DefaultName : ISourceToDestinationNameMapper
    {
        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch) => sourceTypeDetails.GetMember(nameToSearch);
    }
    public class ParentSourceToDestinationNameMapper : IParentSourceToDestinationNameMapper
    {
        public List<ISourceToDestinationNameMapper> NamedMappers { get; } = new(){ new DefaultName() };
        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            foreach (var namedMapper in NamedMappers)
            {
                var memberInfo = namedMapper.GetMatchingMemberInfo(sourceTypeDetails, destType, destMemberType, nameToSearch);
                if (memberInfo != null)
                {
                    return memberInfo;
                }
            }
            return null;
        }
    }
    public class PrePostfixName : ISourceToDestinationNameMapper
    {
        public List<string> DestinationPrefixes { get; } = new();
        public List<string> DestinationPostfixes { get; } = new();
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
    public class ReplaceName : ISourceToDestinationNameMapper
    {
        private readonly List<MemberNameReplacer> _memberNameReplacers = new();

        public ReplaceName AddReplace(string original, string newValue)
        {
            _memberNameReplacers.Add(new(original, newValue));
            return this;
        }
        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            var possibleSourceNames = PossibleNames(nameToSearch);
            if (possibleSourceNames.Length == 0)
            {
                return null;
            }
            var possibleDestNames = sourceTypeDetails.ReadAccessors.Select(mi => (mi, possibles : PossibleNames(mi.Name))).ToArray();
            foreach (var sourceName in possibleSourceNames)
            {
                foreach (var (mi, possibles) in possibleDestNames)
                {
                    if (possibles.Contains(sourceName, StringComparer.OrdinalIgnoreCase))
                    {
                        return mi;
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
    public class MemberNameReplacer
    {
        public MemberNameReplacer(string originalValue, string newValue)
        {
            OriginalValue = originalValue;
            NewValue = newValue;
        }

        public string OriginalValue { get; }
        public string NewValue { get; }
    }
}