using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoMapper.Configuration.Conventions
{
    public class NameSplitMember : IChildMemberConfiguration
    {
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }

        public NameSplitMember()
        {
            SourceMemberNamingConvention = new PascalCaseNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, IMemberConfiguration parent, bool reverseNamingConventions)
        {
            string[] matches;
            if (reverseNamingConventions)
            {
                matches = SourceMemberNamingConvention.SplittingExpression
                    .Matches(nameToSearch)
                    .Cast<Match>()
                    .Select(m => DestinationMemberNamingConvention.ReplaceValue(m))
                    .ToArray();
            }
            else
            {
                matches = DestinationMemberNamingConvention.SplittingExpression
                    .Matches(nameToSearch)
                    .Cast<Match>()
                    .Select(m => SourceMemberNamingConvention.ReplaceValue(m))
                    .ToArray();
            }

            MemberInfo matchingMemberInfo = null;
            for (var i = 1; i <= matches.Length; i++)
            {
                var snippet = CreateNameSnippet(matches, i, reverseNamingConventions);

                matchingMemberInfo = parent.NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, snippet.First);

                if (matchingMemberInfo != null)
                {
                    resolvers.AddLast(matchingMemberInfo);

                    var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                    var foundMatch = parent.MapDestinationPropertyToSource(options, details, destType, destMemberType, snippet.Second, resolvers, reverseNamingConventions);

                    if (!foundMatch)
                        resolvers.RemoveLast();
                    else
                        break;
                }
            }
            return matchingMemberInfo != null;
        }

        private NameSnippet CreateNameSnippet(IEnumerable<string> matches, int i, bool reverseNamingConventions)
        {
            string first;
            string second;
            if (reverseNamingConventions)
            {
                first = string.Join(DestinationMemberNamingConvention.SeparatorCharacter, matches.Take(i).Select(s => DestinationMemberNamingConvention.SplittingExpression.Replace(s, DestinationMemberNamingConvention.ReplaceValue)).ToArray());
                second = string.Join(DestinationMemberNamingConvention.SeparatorCharacter, matches.Skip(i).Select(s => DestinationMemberNamingConvention.SplittingExpression.Replace(s, DestinationMemberNamingConvention.ReplaceValue)).ToArray());
            }
            else
            {
                first = string.Join(SourceMemberNamingConvention.SeparatorCharacter, matches.Take(i).Select(s => SourceMemberNamingConvention.SplittingExpression.Replace(s, SourceMemberNamingConvention.ReplaceValue)).ToArray());
                second = string.Join(SourceMemberNamingConvention.SeparatorCharacter, matches.Skip(i).Select(s => SourceMemberNamingConvention.SplittingExpression.Replace(s, SourceMemberNamingConvention.ReplaceValue)).ToArray());
            }

            return new NameSnippet
            {
                First = first,
                Second = second
            };
        }

        private class NameSnippet
        {
            public string First { get; set; }
            public string Second { get; set; }
        }
    }
}