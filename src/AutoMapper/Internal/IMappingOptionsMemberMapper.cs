using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Impl;
using AutoMapper.Internal;

namespace AutoMapper
{
    public interface IGetTypeInfoMembers
    {
        IEnumerable<MemberInfo> GetMemberInfos(TypeInfo typeInfo);
    }
    public class FieldPropertyMemberInfo : IGetTypeInfoMembers
    {
        public IEnumerable<MemberInfo> GetMemberInfos(TypeInfo typeInfo)
        {
            return typeInfo.PublicReadAccessors;
        }
    }
    public class MethodsMemberInfo : IGetTypeInfoMembers
    {
        public IEnumerable<MemberInfo> GetMemberInfos(TypeInfo typeInfo)
        {
            return typeInfo.PublicNoArgMethods;
        }
    }
    public class AllMemberInfo : IGetTypeInfoMembers
    {
        public IEnumerable<MemberInfo> GetMemberInfos(TypeInfo typeInfo)
        {
            return typeInfo.PublicReadAccessors.Concat(typeInfo.PublicNoArgMethods).Concat(typeInfo.PublicNoArgExtensionMethods);
        }
    }

    public interface IParentSourceToDestinationNameMapper : ISourceToDestinationNameMapper
    {
        ICollection<ISourceToDestinationNameMapper> NamedMappers { get; }
    }

    public class ParentSourceToDestinationNameMapper : IParentSourceToDestinationNameMapper
    {
        private IGetTypeInfoMembers _getMembers;

        public IGetTypeInfoMembers GetMembers
        {
            get { return _getMembers; }
            set
            {
                _getMembers = value;
                foreach (var namedMapper in NamedMappers)
                    namedMapper.GetMembers = value;
            }
        }

        public ICollection<ISourceToDestinationNameMapper> NamedMappers { get; } = new Collection<ISourceToDestinationNameMapper> {new DefaultName()};

        public ParentSourceToDestinationNameMapper()
        {
            GetMembers = SourceToDestinationNameMapperBase.Default;
        }

        public MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, Type destType, string nameToSearch)
        {
            MemberInfo memberInfo = null;
            foreach (var namedMapper in NamedMappers)
            {
                memberInfo = namedMapper.GetMatchingMemberInfo(typeInfo, destType, nameToSearch);
                if (memberInfo != null)
                    break;
            }
            return memberInfo;
        }
    }

    // Source Destination Mapper
    public abstract class SourceToDestinationNameMapperBase : ISourceToDestinationNameMapper
    {
        public IGetTypeInfoMembers GetMembers
        {
            get { return _getMembers; }
            set { _getMembers = value; }
        }

        public static IGetTypeInfoMembers Default = new AllMemberInfo();
        private IGetTypeInfoMembers _getMembers;

        protected SourceToDestinationNameMapperBase()
        {
            GetMembers = Default;
        }

        public abstract MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, Type destType, string nameToSearch);
    }
    public class DefaultName : SourceToDestinationNameMapperBase
    {
        public override MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, Type destType, string nameToSearch)
        {
            return
                GetMembers.GetMemberInfos(typeInfo)
                    .FirstOrDefault(
                        mi =>
                            typeof (ParameterInfo).IsAssignableFrom(destType) 
                                ? string.Compare(mi.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0
                                : string.CompareOrdinal(mi.Name, nameToSearch) == 0);
        }
    }
    public class PrePostfixName : SourceToDestinationNameMapperBase
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

        public override MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, Type destType, string nameToSearch)
        {
            var possibleSourceNames = PossibleNames(nameToSearch, DestinationPrefixes, DestinationPostfixes);
            var possibleDestNames = GetMembers.GetMemberInfos(typeInfo).Select(mi => new { mi, possibles = PossibleNames(mi.Name, Prefixes, Postfixes) });

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new { sourceName, destName };
            var match =
                all.FirstOrDefault(
                    pair => pair.destName.possibles.Any(p => string.CompareOrdinal(p, pair.sourceName) == 0));
            if (match == null)
                return null;
            return match.destName.mi;
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
    public class ReplaceName : SourceToDestinationNameMapperBase
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
        public override MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, Type destType, string nameToSearch)
        {
            var possibleSourceNames = PossibleNames(nameToSearch);
            var possibleDestNames = GetMembers.GetMemberInfos(typeInfo).Select(mi => new { mi, possibles = PossibleNames(mi.Name) });

            var all =
                from sourceName in possibleSourceNames
                from destName in possibleDestNames
                select new { sourceName, destName };
            var match =
                all.FirstOrDefault(
                    pair => pair.destName.possibles.Any(p => string.CompareOrdinal(p, pair.sourceName) == 0));
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

    public interface IChildMemberConfiguration
    {
        IParentSourceToDestinationNameMapper NameMapper { get; set; }
        bool MapDestinationPropertyToSource(TypeInfo sourceType, Type destType, string nameToSearch, LinkedList<MemberInfo> resolvers, IChildMemberConfiguration parent = null);
    }

    public interface IMemberConfiguration : IChildMemberConfiguration
    {
        IList<IChildMemberConfiguration> MemberMappers { get; }
    }

    public class MemberConfiguration : IMemberConfiguration
    {
        private IParentSourceToDestinationNameMapper _nameMapper;

        public IParentSourceToDestinationNameMapper NameMapper
        {
            get { return _nameMapper; }
            set
            {
                _nameMapper = value;
                foreach (var memberMapper in MemberMappers)
                    memberMapper.NameMapper = NameMapper;
            }
        }

        public IList<IChildMemberConfiguration> MemberMappers { get; } = new Collection<IChildMemberConfiguration>();

        public MemberConfiguration()
        {
            NameMapper = new ParentSourceToDestinationNameMapper();
            MemberMappers.Add(new DefaultMember { NameMapper = NameMapper });
        }

        public bool MapDestinationPropertyToSource(TypeInfo sourceType, Type destType, string nameToSearch, LinkedList<MemberInfo> resolvers, IChildMemberConfiguration parent = null)
        {
            var foundMap = false;
            foreach (var memberMapper in MemberMappers)
            {
                foundMap = memberMapper.MapDestinationPropertyToSource(sourceType, destType, nameToSearch, resolvers,this);
                if (foundMap)
                    break;
            }
            return foundMap;
        }
    }

    public abstract class MemberBase : IChildMemberConfiguration
    {
        private IParentSourceToDestinationNameMapper _nameMapper;

        public IParentSourceToDestinationNameMapper NameMapper
        {
            get { return _nameMapper; }
            set { _nameMapper = value; }
        }

        public MemberBase()
        {
            //NameMapper = new ParentSourceToDestinationNameMapper();
        }

        public abstract bool MapDestinationPropertyToSource(TypeInfo sourceType, Type destType, string nameToSearch, LinkedList<MemberInfo> resolvers, IChildMemberConfiguration parent = null);
    }

    public class NameSplitMember : MemberBase
    {
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }

        public IEnumerable<MethodInfo> SourceExtensionMethods
        {
            get { return (Mapper.Configuration as ConfigurationStore).SourceExtensionMethods; }
        }

        public NameSplitMember()
        {
            SourceMemberNamingConvention = new PascalCaseNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }

        public override bool MapDestinationPropertyToSource(TypeInfo sourceType, Type destType, string nameToSearch, LinkedList<MemberInfo> resolvers, IChildMemberConfiguration parent = null)
        {
            string[] matches = DestinationMemberNamingConvention.SplittingExpression
                       .Matches(nameToSearch)
                       .Cast<Match>()
                       .Select(m => m.Value)
                       .ToArray();
            MemberInfo matchingMemberInfo = null;
            for (int i = 1; i <= matches.Length; i++)
            {
                NameSnippet snippet = CreateNameSnippet(matches, i);

                matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, destType, snippet.First);

                if (matchingMemberInfo != null)
                {
                    resolvers.AddLast(matchingMemberInfo);

                    var foundMatch = parent.MapDestinationPropertyToSource(TypeMapFactory.GetTypeInfo(matchingMemberInfo.GetMemberType(), SourceExtensionMethods), destType, snippet.Second, resolvers);

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
            var first = String.Join(SourceMemberNamingConvention.SeparatorCharacter, matches.Take(i).Select(s => SourceMemberNamingConvention.SplittingExpression.Replace(s, SourceMemberNamingConvention.ReplaceValue)).ToArray());
            var second = String.Join(SourceMemberNamingConvention.SeparatorCharacter, matches.Skip(i).Select(s => SourceMemberNamingConvention.SplittingExpression.Replace(s, SourceMemberNamingConvention.ReplaceValue)).ToArray());
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

    public class DefaultMember : MemberBase
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public override bool MapDestinationPropertyToSource(TypeInfo sourceType, Type destType, string nameToSearch, LinkedList<MemberInfo> resolvers, IChildMemberConfiguration parent = null)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            var matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, destType, nameToSearch);

            if (matchingMemberInfo != null)
                resolvers.AddLast(matchingMemberInfo);
            return matchingMemberInfo != null;
        }
    }

    public static class ConvensionExtensions
    {
        public static IMemberConfiguration AddMember<TMemberMapper>(this IMemberConfiguration self, Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new()
        {
            var child = self.GetOrAdd(_ => (IList)_.MemberMappers, setupAction);
            child.NameMapper = self.NameMapper;
            return self;
        }

        public static IMemberConfiguration AddName<TNameMapper>(this IMemberConfiguration self, Action<TNameMapper> setupAction = null)
            where TNameMapper : ISourceToDestinationNameMapper, new()
        {
            var child = self.GetOrAdd(_ => (IList)_.NameMapper.NamedMappers, setupAction);
            child.GetMembers = self.NameMapper.GetMembers;
            return self;
        }

        public static IMemberConfiguration SetMemberInfo<TMember>(this IMemberConfiguration self, Action<TMember> setupAction = null)
            where TMember : IGetTypeInfoMembers, new()
        {
            var nameMapper = new TMember();
            setupAction?.Invoke(nameMapper);
            self.NameMapper.GetMembers = nameMapper;
            return self;
        }

        private static TMemberMapper GetOrAdd<TMemberMapper>(this 
            IMemberConfiguration self, Func<IMemberConfiguration,IList> getList, Action<TMemberMapper> setupAction = null)
            where TMemberMapper : new()
        {
            var child = getList(self).OfType<TMemberMapper>().FirstOrDefault();
            if (child == null)
            {
                child = new TMemberMapper();
                getList(self).Add(child);
            }
            setupAction?.Invoke(child);
            return child;
        }
    }
}