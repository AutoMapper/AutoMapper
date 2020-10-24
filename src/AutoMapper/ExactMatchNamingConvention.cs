using System.Text.RegularExpressions;

namespace AutoMapper
{
    public class ExactMatchNamingConvention : INamingConvention
    {
        public static readonly ExactMatchNamingConvention Instance = new ExactMatchNamingConvention();
        public Regex SplittingExpression { get; }
        public string SeparatorCharacter => "";
        public string ReplaceValue(Match match) => match.Value;
    }
}