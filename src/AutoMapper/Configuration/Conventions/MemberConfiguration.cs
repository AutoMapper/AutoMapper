using AutoMapper.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
namespace AutoMapper.Configuration.Conventions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IMemberConfiguration
    {
        List<IChildMemberConfiguration> MemberMappers { get; }
        IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null) where TMemberMapper : IChildMemberConfiguration, new();
        IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null) where TNameMapper : ISourceToDestinationNameMapper, new();
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
        public List<IChildMemberConfiguration> MemberMappers { get; } = new();
        public IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null) where TMemberMapper : IChildMemberConfiguration, new() =>
            GetOrAdd(MemberMappers, setupAction);
        public IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null) where TNameMapper : ISourceToDestinationNameMapper, new() =>
            GetOrAdd(NameMapper.NamedMappers, setupAction);
        private IMemberConfiguration GetOrAdd<TMemberMapper>(IList list, Action<TMemberMapper> setupAction = null) where TMemberMapper : new()
        {
            var child = list.OfType<TMemberMapper>().FirstOrDefault();
            if (child == null)
            {
                child = new();
                list.Add(child);
            }
            setupAction?.Invoke(child);
            return this;
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
        bool _default = true;
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }
        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, IMemberConfiguration parent, bool isReverseMap) =>
            _default ?
                Default(options, sourceType, destType, destMemberType, nameToSearch, resolvers, parent, isReverseMap) :
                Conventions(options, sourceType, destType, destMemberType, nameToSearch, resolvers, parent, isReverseMap);
        bool Default(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, IMemberConfiguration parent, bool isReverseMap)
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
                matchingMemberInfo = parent.NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, first);
                if (matchingMemberInfo == null)
                {
                    return false;
                }
                resolvers.Add(matchingMemberInfo);
                var second = nameToSearch[index..];
                var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                if (parent.MapDestinationPropertyToSource(options, details, destType, destMemberType, second, resolvers, isReverseMap))
                {
                    return true;
                }
                resolvers.RemoveAt(resolvers.Count - 1);
                return false;
            }
        }
        bool Conventions(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, IMemberConfiguration parent, bool isReverseMap)
        {
            var destinationMemberNamingConvention = isReverseMap ? SourceMemberNamingConvention : DestinationMemberNamingConvention;
            var matches = destinationMemberNamingConvention.Split(nameToSearch);
            var length = matches.Length;
            if (length < 2)
            {
                return false;
            }
            var sourceMemberNamingConvention = isReverseMap ? DestinationMemberNamingConvention : SourceMemberNamingConvention;
            var separator = sourceMemberNamingConvention.SeparatorCharacter;
            for (var index = 1; index <= length; index++)
            {
                var first = string.Join(separator, matches, 0, index);
                var matchingMemberInfo = parent.NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, first);
                if (matchingMemberInfo != null)
                {
                    resolvers.Add(matchingMemberInfo);
                    var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                    var second = string.Join(separator, matches, index, length - index);
                    if (parent.MapDestinationPropertyToSource(options, details, destType, destMemberType, second, resolvers, isReverseMap))
                    {
                        return true;
                    }
                    resolvers.RemoveAt(resolvers.Count - 1);
                }
            }
            return false;
        }
        internal void Set(INamingConvention source, INamingConvention destination)
        {
            if (source == null)
            {
                SourceMemberNamingConvention = PascalCaseNamingConvention.Instance;
            }
            else
            {
                SourceMemberNamingConvention = source;
                _default = false;
            }
            if (destination == null)
            {
                DestinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
            }
            else
            {
                DestinationMemberNamingConvention = destination;
                _default = false;
            }
        }
    }
}