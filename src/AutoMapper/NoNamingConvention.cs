using System;
using System.Text.RegularExpressions;

namespace AutoMapper
{
    public class NoNamingConvention : INamingConvention
    {
        public static readonly INamingConvention Value = new NoNamingConvention();

        private NoNamingConvention()
        {
        }

        public Regex SplittingExpression { get; } = new Regex(".+");

        public string SeparatorCharacter => "";

        public string ReplaceValue(Match match)
        {
            return match.Value;
        }
    }
}