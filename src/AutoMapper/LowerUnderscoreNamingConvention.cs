using System.Text.RegularExpressions;

namespace AutoMapper
{
    public class LowerUnderscoreNamingConvention : INamingConvention
    {
        private static readonly Regex LowerUnderscore = new Regex(@"[\p{Ll}\p{Lu}0-9]+(?=_?)");
        public static readonly LowerUnderscoreNamingConvention Instance = new LowerUnderscoreNamingConvention();
        public Regex SplittingExpression { get; } = LowerUnderscore;
        public string SeparatorCharacter => "_";
        public string ReplaceValue(Match match) => match.Value.ToLower();
    }
}