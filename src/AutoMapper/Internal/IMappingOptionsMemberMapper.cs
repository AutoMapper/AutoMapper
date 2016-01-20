using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Internal;

namespace AutoMapper
{
    public interface IGetTypeInfoMembers
    {
        IEnumerable<MemberInfo> GetMemberInfos(TypeDetails typeInfo);
        IGetTypeInfoMembers AddCondition(Func<MemberInfo, bool> predicate);
    }
    public class AllMemberInfo : IGetTypeInfoMembers
    {
        private readonly IList<Func<MemberInfo,bool>> _predicates = new List<Func<MemberInfo, bool>>();

        public IEnumerable<MemberInfo> GetMemberInfos(TypeDetails typeInfo)
        {
            return AllMembers(typeInfo).Where(m => _predicates.All(p => p(m))).ToList();
        }

        public IGetTypeInfoMembers AddCondition(Func<MemberInfo, bool> predicate)
        {
            _predicates.Add(predicate);
            return this;
        }

        private static IEnumerable<MemberInfo> AllMembers(TypeDetails typeInfo)
        {
            return typeInfo.PublicReadAccessors.Concat(typeInfo.PublicNoArgMethods).Concat(typeInfo.PublicNoArgExtensionMethods).ToList();
        }
    }

    public interface IParentSourceToDestinationNameMapper
    {
        ICollection<ISourceToDestinationNameMapper> NamedMappers { get; }
        IGetTypeInfoMembers GetMembers { get; }
        MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, string nameToSearch);
    }
    public class ParentSourceToDestinationNameMapper : IParentSourceToDestinationNameMapper
    {
        public IGetTypeInfoMembers GetMembers { get; } = new AllMemberInfo();

        public ICollection<ISourceToDestinationNameMapper> NamedMappers { get; } = new Collection<ISourceToDestinationNameMapper> {new DefaultName(), new SourceToDestinationNameMapperAttributesMember()};

        public MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, string nameToSearch)
        {
            MemberInfo memberInfo = null;
            foreach (var namedMapper in NamedMappers)
            {
                memberInfo = namedMapper.GetMatchingMemberInfo(GetMembers, typeInfo, destType, nameToSearch);
                if (memberInfo != null)
                    break;
            }
            return memberInfo;
        }
    }

    // Source Destination Mapper
    public class DefaultName : CaseSensitiveName
    {
    }

    public class CaseSensitiveName : ISourceToDestinationNameMapper
    {
        public bool MethodCaseSensitive { get; set; }

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, string nameToSearch)
        {
            return
                getTypeInfoMembers.GetMemberInfos(typeInfo)
                    .FirstOrDefault(
                        mi =>
                            typeof (ParameterInfo).IsAssignableFrom(destType) || !MethodCaseSensitive
                                ? string.Compare(mi.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0
                                : string.CompareOrdinal(mi.Name, nameToSearch) == 0);
        }
    }

    public class CaseInsensitiveName : ISourceToDestinationNameMapper
    {
        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, string nameToSearch)
        {
            return
                getTypeInfoMembers.GetMemberInfos(typeInfo)
                    .FirstOrDefault(mi => string.Compare(mi.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
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

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, string nameToSearch)
        {
            var possibleSourceNames = PossibleNames(nameToSearch, DestinationPrefixes, DestinationPostfixes);
            var possibleDestNames = getTypeInfoMembers.GetMemberInfos(typeInfo).Select(mi => new { mi, possibles = PossibleNames(mi.Name, Prefixes, Postfixes) });

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new { sourceName, destName };
            var match =
                all.FirstOrDefault(
                    pair => pair.destName.possibles.Any(p => string.Compare(p, pair.sourceName, StringComparison.OrdinalIgnoreCase) == 0));
            return match?.destName.mi;
        }

        private IEnumerable<string> PossibleNames(string memberName, IEnumerable<string> prefixes, IEnumerable<string> postfixes)
        {
            if (string.IsNullOrEmpty(memberName))
                yield break;

            yield return memberName;

            foreach (var withoutPrefix in prefixes.Where(prefix => memberName.StartsWith(prefix, StringComparison.Ordinal)).Select(prefix => memberName.Substring(prefix.Length)))
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
                postfixes.Where(postfix => name.EndsWith(postfix, StringComparison.Ordinal))
                    .Select(postfix => name.Remove(name.Length - postfix.Length));
        }
    }
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
        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, string nameToSearch)
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
    public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
    {
        private static readonly ConcurrentDictionary<TypeDetails, Dictionary<MemberInfo, IEnumerable<SourceToDestinationMapperAttribute>>> Cache = new ConcurrentDictionary<TypeDetails, Dictionary<MemberInfo, IEnumerable<SourceToDestinationMapperAttribute>>>();

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, string nameToSearch)
        {
            Cache.GetOrAdd(typeInfo, ti => getTypeInfoMembers.GetMemberInfos(ti).ToDictionary(mi => mi, mi => mi.GetCustomAttributes(typeof(SourceToDestinationMapperAttribute), true).OfType<SourceToDestinationMapperAttribute>()));

            return Cache[typeInfo].FirstOrDefault(kp => kp.Value.Any(_ => _.IsMatch(typeInfo, kp.Key, destType, nameToSearch))).Key;
        }
    }

    public abstract class SourceToDestinationMapperAttribute : Attribute
    {
        public abstract bool IsMatch(TypeDetails typeInfo, MemberInfo memberInfo, Type destType, string nameToSearch);
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MapToAttribute : SourceToDestinationMapperAttribute
    {
        public string MatchingName { get; }

        public MapToAttribute(string matchingName)
        {
            MatchingName = matchingName;
        }

        public override bool IsMatch(TypeDetails typeInfo, MemberInfo memberInfo, Type destType, string nameToSearch)
        {
            return string.Compare(MatchingName, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    public interface IMemberConfiguration
    {
        IList<IChildMemberConfiguration> MemberMappers { get; }
        IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new();

        IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
            where TNameMapper : ISourceToDestinationNameMapper, new();

        IParentSourceToDestinationNameMapper NameMapper { get; set; }
        bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers);
    }
    public class MemberConfiguration : IMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public IList<IChildMemberConfiguration> MemberMappers { get; } = new Collection<IChildMemberConfiguration>();
        
        public IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new()
        {
            GetOrAdd(_ => (IList)_.MemberMappers, setupAction);
            return this;
        }

        public IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
            where TNameMapper : ISourceToDestinationNameMapper, new()
        {
            GetOrAdd(_ => (IList)_.NameMapper.NamedMappers, setupAction);
            return this;
        }

        private TMemberMapper GetOrAdd<TMemberMapper>(Func<IMemberConfiguration, IList> getList, Action<TMemberMapper> setupAction = null)
            where TMemberMapper : new()
        {
            var child = getList(this).OfType<TMemberMapper>().FirstOrDefault();
            if (child == null)
            {
                child = new TMemberMapper();
                getList(this).Add(child);
            }
            setupAction?.Invoke(child);
            return child;
        }

        public MemberConfiguration()
        {
            NameMapper = new ParentSourceToDestinationNameMapper();
            MemberMappers.Add(new DefaultMember { NameMapper = NameMapper });
        }

        public bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers)
        {
            var foundMap = false;
            foreach (var memberMapper in MemberMappers)
            {
                foundMap = memberMapper.MapDestinationPropertyToSource(options, sourceType, destType, nameToSearch, resolvers, this);
                if (foundMap)
                    break;
            }
            return foundMap;
        }
    }

    public interface IChildMemberConfiguration
    {
        bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers, IMemberConfiguration parent);
    }
    public class NameSplitMember : IChildMemberConfiguration
    {
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }

        public NameSplitMember()
        {
            SourceMemberNamingConvention = new PascalCaseNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }

        public bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers, IMemberConfiguration parent )
        {
            string[] matches = DestinationMemberNamingConvention.SplittingExpression
                       .Matches(nameToSearch)
                       .Cast<Match>()
                       .Select(m => SourceMemberNamingConvention.ReplaceValue(m))
                       .ToArray();
            MemberInfo matchingMemberInfo = null;
            for (int i = 1; i <= matches.Length; i++)
            {
                NameSnippet snippet = CreateNameSnippet(matches, i);

                matchingMemberInfo = parent.NameMapper.GetMatchingMemberInfo(sourceType, destType, snippet.First);

                if (matchingMemberInfo != null)
                {
                    resolvers.AddLast(matchingMemberInfo.ToMemberGetter());

                    var foundMatch = parent.MapDestinationPropertyToSource(options, TypeMapFactory.GetTypeInfo(matchingMemberInfo.GetMemberType(), options), destType, snippet.Second, resolvers);

                    if (!foundMatch)
                        resolvers.RemoveLast();
                    else
                        break;
                }
            }
            return matchingMemberInfo != null;
        }
        private NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i)
        {
            var first = string.Join(SourceMemberNamingConvention.SeparatorCharacter, matches.Take(i).Select(s => SourceMemberNamingConvention.SplittingExpression.Replace(s, SourceMemberNamingConvention.ReplaceValue)).ToArray());
            var second = string.Join(SourceMemberNamingConvention.SeparatorCharacter, matches.Skip(i).Select(s => SourceMemberNamingConvention.SplittingExpression.Replace(s, SourceMemberNamingConvention.ReplaceValue)).ToArray());
            return new NameSnippet
            {
                First = first,
                Second =second
            };
        }
        private class NameSnippet
        {
            public string First { get; set; }
            public string Second { get; set; }
        }
    }
    public class DefaultMember : IChildMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers, IMemberConfiguration parent = null)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            var matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, destType, nameToSearch);

            if (matchingMemberInfo != null)
                resolvers.AddLast(matchingMemberInfo.ToMemberGetter());
            return matchingMemberInfo != null;
        }
    }
}