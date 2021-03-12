using AutoMapper.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoMapper.Configuration.Conventions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IMemberConfiguration
    {
        List<IChildMemberConfiguration> MemberMappers { get; }
        IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new();

        IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
            where TNameMapper : ISourceToDestinationNameMapper, new();

        IParentSourceToDestinationNameMapper NameMapper { get; set; }
        bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IChildMemberConfiguration
    {
        bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, IMemberConfiguration parent, bool isReverseMap);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DefaultMember : IChildMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, IMemberConfiguration parent = null, bool isReverseMap = false)
        {
            var matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceTypeDetails, destType, destMemberType, nameToSearch);
            if (matchingMemberInfo != null)
            {
                resolvers.Add(matchingMemberInfo);
                return true;
            }
            return nameToSearch.Length == 0;
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberConfiguration : IMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public List<IChildMemberConfiguration> MemberMappers { get; } = new List<IChildMemberConfiguration>();

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

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap)
        {
            foreach (var memberMapper in MemberMappers)
            {
                if (memberMapper.MapDestinationPropertyToSource(options, sourceType, destType, destMemberType, nameToSearch, resolvers, this, isReverseMap))
                {
                    return true;
                }
            }
            return false;
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NameSplitMember : IChildMemberConfiguration
    {
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, IMemberConfiguration parent, bool isReverseMap)
        {
            var destinationMemberNamingConvention = isReverseMap
                ? SourceMemberNamingConvention
                : DestinationMemberNamingConvention;
            var sourceMemberNamingConvention = isReverseMap
                ? DestinationMemberNamingConvention
                : SourceMemberNamingConvention;

            var matches = destinationMemberNamingConvention.SplittingExpression
                ?.Matches(nameToSearch)
                .Cast<Match>()
                .Select(m => sourceMemberNamingConvention.ReplaceValue(m))
                .ToArray()
                ?? Array.Empty<string>();

            MemberInfo matchingMemberInfo = null;
            for (var i = 1; i <= matches.Length; i++)
            {
                var first = string.Join(
                    sourceMemberNamingConvention.SeparatorCharacter,
                    matches.Take(i).Select(SplitMembers));
                var second = string.Join(
                    sourceMemberNamingConvention.SeparatorCharacter,
                    matches.Skip(i).Select(SplitMembers));

                matchingMemberInfo = parent.NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, first);

                if (matchingMemberInfo != null)
                {
                    resolvers.Add(matchingMemberInfo);

                    var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                    var foundMatch = parent.MapDestinationPropertyToSource(options, details, destType, destMemberType, second, resolvers, isReverseMap);

                    if (!foundMatch)
                        resolvers.RemoveAt(resolvers.Count - 1);
                    else
                        break;
                }
            }
            return matchingMemberInfo != null;
            string SplitMembers(string value) => sourceMemberNamingConvention.SplittingExpression.Replace(value, sourceMemberNamingConvention.ReplaceValue);
        }
    }
}