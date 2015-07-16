using System;
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
            return typeInfo.GetPublicReadAccessors();
        }
    }
    public class MethodsMemberInfo : IGetTypeInfoMembers
    {
        public IEnumerable<MemberInfo> GetMemberInfos(TypeInfo typeInfo)
        {
            return typeInfo.GetPublicNoArgMethods();
        }
    }
    public class AllMemberInfo : IGetTypeInfoMembers
    {
        public IEnumerable<MemberInfo> GetMemberInfos(TypeInfo typeInfo)
        {
            return typeInfo.GetPublicReadAccessors().Concat(typeInfo.GetPublicNoArgMethods()).Concat(typeInfo.GetPublicNoArgExtensionMethods());
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

        public ICollection<ISourceToDestinationNameMapper> NamedMappers { get; private set; }

        public ParentSourceToDestinationNameMapper()
        {
            NamedMappers = new Collection<ISourceToDestinationNameMapper>();
            NamedMappers.Add(new DefaultName());
            GetMembers = SourceToDestinationNameMapperBase.Default;
        }

        public MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, string nameToSearch)
        {
            MemberInfo memberInfo = null;
            foreach (var namedMapper in NamedMappers)
            {
                memberInfo = namedMapper.GetMatchingMemberInfo(typeInfo, nameToSearch);
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

        public abstract MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, string nameToSearch);
    }
    public class DefaultName : SourceToDestinationNameMapperBase
    {
        public override MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, string nameToSearch)
        {
            return GetMembers.GetMemberInfos(typeInfo).FirstOrDefault(mi => string.Compare(mi.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
    public class PrePostfixName : SourceToDestinationNameMapperBase
    {
        public ICollection<string> Prefixes { get; private set; }
        public ICollection<string> Postfixes { get; private set; }
        public ICollection<string> DestinationPrefixes { get; private set; }
        public ICollection<string> DestinationPostfixes { get; private set; }

        public PrePostfixName()
        {
            Prefixes = new Collection<string>();
            Postfixes = new Collection<string>();
            DestinationPrefixes = new Collection<string>();
            DestinationPostfixes = new Collection<string>();
        }

        public override MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, string nameToSearch)
        {
            var possibleSourceNames = PossibleNames(nameToSearch, Prefixes, Postfixes);
            var possibleDestNames = GetMembers.GetMemberInfos(typeInfo).Select(mi => new { mi, possibles = PossibleNames(mi.Name, DestinationPrefixes, DestinationPostfixes) });

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
        public ICollection<MemberNameReplacer> MemberNameReplacers { get; private set; }

        public ReplaceName()
        {
            MemberNameReplacers = new Collection<MemberNameReplacer>();
        }

        public override MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, string nameToSearch)
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

        public IEnumerable<string> PossibleNames(string nameToSearch)
        {
            return 
                MemberNameReplacers.Select(r => nameToSearch.Replace(r.OrginalValue, r.NewValue))
                    .Concat(new[] { MemberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OrginalValue, r.NewValue)), nameToSearch })
                    .ToList();
        }
    }

    public interface ISourceToDestinationMemberMapper
    {
        IParentSourceToDestinationNameMapper NameMapper { get; set; }
        bool MapDestinationPropertyToSource(TypeInfo sourceType, string nameToSearch, LinkedList<MemberInfo> resolvers, ISourceToDestinationMemberMapper parent = null);
    }

    public interface IParentSourceToDestinationMemberMapper : ISourceToDestinationMemberMapper
    {
        ICollection<ISourceToDestinationMemberMapper> MemberMappers { get; }
    }

    public class ParentSourceToDestinationMemberMapper : IParentSourceToDestinationMemberMapper
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

        public ICollection<ISourceToDestinationMemberMapper> MemberMappers { get; private set; }

        public ParentSourceToDestinationMemberMapper()
        {
            MemberMappers = new Collection<ISourceToDestinationMemberMapper>();
            NameMapper = new ParentSourceToDestinationNameMapper();
            MemberMappers.Add(new DefaultMember { NameMapper = NameMapper });
        }

        public bool MapDestinationPropertyToSource(TypeInfo sourceType, string nameToSearch, LinkedList<MemberInfo> resolvers, ISourceToDestinationMemberMapper parent = null)
        {
            var foundMap = false;
            foreach (var memberMapper in MemberMappers)
            {
                foundMap = memberMapper.MapDestinationPropertyToSource(sourceType, nameToSearch, resolvers, this);
                if (foundMap)
                    break;
            }
            return foundMap;
        }
    }

    public abstract class MemberBase : ISourceToDestinationMemberMapper
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

        public abstract bool MapDestinationPropertyToSource(TypeInfo sourceType, string nameToSearch, LinkedList<MemberInfo> resolvers,ISourceToDestinationMemberMapper parent = null);
    }

    public class NameSplitMember : MemberBase
    {
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }

        public IEnumerable<MethodInfo> SourceExtensionMethods
        {
            get { return Mapper.Configuration.SourceExtensionMethods; }
        }

        public NameSplitMember()
        {
            SourceMemberNamingConvention = new PascalCaseNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }

        public override bool MapDestinationPropertyToSource(TypeInfo sourceType, string nameToSearch, LinkedList<MemberInfo> resolvers, ISourceToDestinationMemberMapper parent)
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

                matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, snippet.First);

                if (matchingMemberInfo != null)
                {
                    resolvers.AddLast(matchingMemberInfo);

                    var foundMatch = parent.MapDestinationPropertyToSource(TypeMapFactory.GetTypeInfo(matchingMemberInfo.GetMemberType(), SourceExtensionMethods), snippet.Second, resolvers);

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
            return new NameSnippet
            {
                First =
                    String.Join(SourceMemberNamingConvention.SeparatorCharacter,
                                matches.Take(i).ToArray()),
                Second =
                    String.Join(SourceMemberNamingConvention.SeparatorCharacter,
                                matches.Skip(i).ToArray())
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

        public override bool MapDestinationPropertyToSource(TypeInfo sourceType, string nameToSearch, LinkedList<MemberInfo> resolvers, ISourceToDestinationMemberMapper parent)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            var matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, nameToSearch);

            if (matchingMemberInfo != null)
                resolvers.AddLast(matchingMemberInfo);
            return matchingMemberInfo != null;
        }
    }

    public static class ConvensionExtensions
    {
        public static IParentSourceToDestinationMemberMapper AddMember<TMemberMapper>(this IParentSourceToDestinationMemberMapper self, Action<TMemberMapper> setupAction = null)
            where TMemberMapper : ISourceToDestinationMemberMapper, new()
        {
            var child = new TMemberMapper();
            if (setupAction != null)
                setupAction(child);
            self.MemberMappers.Add(child);
            child.NameMapper = self.NameMapper;
            return self;
        }
        public static IParentSourceToDestinationMemberMapper AddName<TNameMapper>(this IParentSourceToDestinationMemberMapper self, Action<TNameMapper> setupAction = null)
           where TNameMapper : ISourceToDestinationNameMapper, new()
        {
            var nameMapper = new TNameMapper();
            if(setupAction != null)
                setupAction(nameMapper);
            self.NameMapper.NamedMappers.Add(nameMapper);
            
            nameMapper.GetMembers = self.NameMapper.GetMembers;
            return self;
        }
        public static IParentSourceToDestinationMemberMapper SetMemberInfo<TMember>(this IParentSourceToDestinationMemberMapper self, Action<TMember> setupAction = null)
           where TMember : IGetTypeInfoMembers, new()
        {
            var nameMapper = new TMember();
            if(setupAction != null)
                setupAction(nameMapper);
            self.NameMapper.GetMembers = nameMapper;
            return self;
        }

        public static PrePostfixName SetPrefixs(this PrePostfixName self, params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                self.Prefixes.Add(prefix);
                self.DestinationPrefixes.Add(prefix);
            }

            return self;
        }

        public static PrePostfixName SetPostfixs(this PrePostfixName self, params string[] postfixes)
        {
            foreach (var postfix in postfixes)
            {
                self.Postfixes.Add(postfix);
                self.DestinationPostfixes.Add(postfix);
            }
            return self;
        }

        public static ReplaceName AddReplace(this ReplaceName self, string originalValue, string newValue)
        {
            self.MemberNameReplacers.Add(new MemberNameReplacer(originalValue, newValue));
            return self;
        }
    }
}