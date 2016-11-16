namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Execution;

    public class NameSplitMember : IChildMemberConfiguration
    {
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }

        public NameSplitMember()
        {
            SourceMemberNamingConvention = new PascalCaseNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, IMemberConfiguration parent )
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

                matchingMemberInfo = parent.NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, snippet.First);

                if (matchingMemberInfo != null)
                {
                    resolvers.AddLast(matchingMemberInfo);

                    var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                    var foundMatch = parent.MapDestinationPropertyToSource(options, details, destType, destMemberType, snippet.Second, resolvers);

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
}