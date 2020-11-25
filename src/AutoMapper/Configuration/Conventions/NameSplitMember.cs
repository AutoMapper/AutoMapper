using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoMapper.Configuration.Conventions
{
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