using AutoMapper.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }
        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, IMemberConfiguration parent, bool isReverseMap)
        {
            var destinationMemberNamingConvention = isReverseMap ? SourceMemberNamingConvention : DestinationMemberNamingConvention;
            var matches = destinationMemberNamingConvention.Split(nameToSearch);
            if (matches == null)
            {
                return false;
            }
            var sourceMemberNamingConvention = isReverseMap ? DestinationMemberNamingConvention : SourceMemberNamingConvention;
            var separator = sourceMemberNamingConvention.SeparatorCharacter;
            var length = matches.Count - 1;
            for (var index = 0; index <= length; index++)
            {
                var first = Join(separator, matches, 0, index);
                var matchingMemberInfo = parent.NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, first);
                if (matchingMemberInfo == null)
                {
                    continue;
                }
                resolvers.Add(matchingMemberInfo);
                if (index == length)
                {
                    return true;
                }
                var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                var second = Join(separator, matches, index + 1, length);
                if (parent.MapDestinationPropertyToSource(options, details, destType, destMemberType, second, resolvers, isReverseMap))
                {
                    return true;
                }
                resolvers.RemoveAt(resolvers.Count - 1);
            }
            return false;
            static string Join(string separator, List<ReadOnlyMemory<char>> matches, int startIndex, int endIndex)
            {
                Debug.Assert(startIndex <= endIndex);
                int resultLength = 0;
                var separatorLength = separator.Length;
                for (int index = startIndex; index <= endIndex; index++)
                {
                    resultLength += matches[index].Length + separatorLength;
                }
                resultLength -= separatorLength;
                return string.Create(resultLength, (matches, startIndex, endIndex, separator), static (span, state) =>
                {
                    var separatorSpan = state.separator.AsSpan();
                    for (int index = state.startIndex, destinationIndex = 0; index <= state.endIndex; index++)
                    {
                        var match = state.matches[index].Span;
                        match.CopyTo(span[destinationIndex..]);
                        destinationIndex += match.Length;
                        if (index != state.endIndex)
                        {
                            separatorSpan.CopyTo(span[destinationIndex..]);
                            destinationIndex += separatorSpan.Length;
                        }
                    }
                });
            }
        }
    }
}