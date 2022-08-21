using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
namespace AutoMapper.Configuration.Conventions
{
    public interface ISourceToDestinationNameMapper
    {
        MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch);
        void Merge(ISourceToDestinationNameMapper otherNamedMapper);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberConfiguration
    {
        NameSplitMember _nameSplitMember;
        public INamingConvention SourceMemberNamingConvention { get; set; } = PascalCaseNamingConvention.Instance;
        public INamingConvention DestinationMemberNamingConvention { get; set; } = PascalCaseNamingConvention.Instance;
        public List<ISourceToDestinationNameMapper> NameToMemberMappers { get; } = new();
        public bool IsMatch(ProfileMap options, TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap)
        {
            var matchingMemberInfo = GetSourceMember(sourceTypeDetails, destType, destMemberType, nameToSearch);
            if (matchingMemberInfo != null)
            {
                resolvers.Add(matchingMemberInfo);
                return true;
            }
            return nameToSearch.Length == 0 || _nameSplitMember.IsMatch(options, sourceTypeDetails, destType, destMemberType, nameToSearch, resolvers, isReverseMap);
        }
        public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            var sourceMember = sourceTypeDetails.GetMember(nameToSearch);
            if (sourceMember != null)
            {
                return sourceMember;
            }
            foreach (var namedMapper in NameToMemberMappers)
            {
                if ((sourceMember = namedMapper.GetSourceMember(sourceTypeDetails, destType, destMemberType, nameToSearch)) != null)
                {
                    return sourceMember;
                }
            }
            return null;
        }
        public void Seal()
        {
            var isDefault = SourceMemberNamingConvention == PascalCaseNamingConvention.Instance && DestinationMemberNamingConvention == PascalCaseNamingConvention.Instance;
            _nameSplitMember = isDefault ? new DefaultNameSplitMember() : new ConventionsNameSplitMember();
            _nameSplitMember.Parent = this;
        }
        public void Merge(MemberConfiguration other)
        {
            if (other == null)
            {
                return;
            }
            var initialCount = NameToMemberMappers.Count;
            for (int index = 0; index < other.NameToMemberMappers.Count; index++)
            {
                var otherNamedMapper = other.NameToMemberMappers[index];
                if (index < initialCount)
                {
                    var namedMapper = NameToMemberMappers[index];
                    if (namedMapper.GetType() == otherNamedMapper.GetType())
                    {
                        namedMapper.Merge(otherNamedMapper);
                        continue;
                    }
                }
                NameToMemberMappers.Add(otherNamedMapper);
            }
        }
    }
    public class PrePostfixName : ISourceToDestinationNameMapper
    {
        public HashSet<string> DestinationPrefixes { get; } = new();
        public HashSet<string> DestinationPostfixes { get; } = new();
        public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
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
        public void Merge(ISourceToDestinationNameMapper otherNamedMapper)
        {
            var typedOther = (PrePostfixName)otherNamedMapper;
            DestinationPrefixes.UnionWith(typedOther.DestinationPrefixes);
            DestinationPostfixes.UnionWith(typedOther.DestinationPostfixes);
        }
    }
    public class ReplaceName : ISourceToDestinationNameMapper
    {
        public HashSet<MemberNameReplacer> MemberNameReplacers { get; } = new();
        public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
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
        public void Merge(ISourceToDestinationNameMapper otherNamedMapper)
        {
            var typedOther = (ReplaceName)otherNamedMapper;
            MemberNameReplacers.UnionWith(typedOther.MemberNameReplacers);
        }
        private string[] PossibleNames(string nameToSearch) =>
                MemberNameReplacers.Select(r => nameToSearch.Replace(r.OriginalValue, r.NewValue))
                    .Concat(new[] { MemberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OriginalValue, r.NewValue)), nameToSearch })
                    .ToArray();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct MemberNameReplacer(string OriginalValue, string NewValue);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class NameSplitMember
    {
        public MemberConfiguration Parent { get; set; }
        public abstract bool IsMatch(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DefaultNameSplitMember : NameSplitMember
    {
        public sealed override bool IsMatch(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap)
        {
            MemberInfo matchingMemberInfo = null;
            int index = 1;
            for (; index < nameToSearch.Length; index++)
            {
                if (char.IsUpper(nameToSearch[index]) && Found())
                {
                    return true;
                }
            }
            return matchingMemberInfo != null && Found();
            bool Found()
            {
                var first = nameToSearch[..index];
                matchingMemberInfo = Parent.GetSourceMember(sourceType, destType, destMemberType, first);
                if (matchingMemberInfo == null)
                {
                    return false;
                }
                resolvers.Add(matchingMemberInfo);
                var second = nameToSearch[index..];
                var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                if (Parent.IsMatch(options, details, destType, destMemberType, second, resolvers, isReverseMap))
                {
                    return true;
                }
                resolvers.RemoveAt(resolvers.Count - 1);
                return false;
            }
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ConventionsNameSplitMember : NameSplitMember
    {
        public sealed override bool IsMatch(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap)
        {
            var destinationMemberNamingConvention = isReverseMap ? Parent.SourceMemberNamingConvention : Parent.DestinationMemberNamingConvention;
            var matches = destinationMemberNamingConvention.Split(nameToSearch);
            var length = matches.Length;
            if (length < 2)
            {
                return false;
            }
            var sourceMemberNamingConvention = isReverseMap ? Parent.DestinationMemberNamingConvention : Parent.SourceMemberNamingConvention;
            var separator = sourceMemberNamingConvention.SeparatorCharacter;
            for (var index = 1; index <= length; index++)
            {
                var first = string.Join(separator, matches, 0, index);
                var matchingMemberInfo = Parent.GetSourceMember(sourceType, destType, destMemberType, first);
                if (matchingMemberInfo != null)
                {
                    resolvers.Add(matchingMemberInfo);
                    var second = string.Join(separator, matches, index, length - index);
                    var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                    if (Parent.IsMatch(options, details, destType, destMemberType, second, resolvers, isReverseMap))
                    {
                        return true;
                    }
                    resolvers.RemoveAt(resolvers.Count - 1);
                }
            }
            return false;
        }
    }
}