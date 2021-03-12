using System.Text.RegularExpressions;
namespace AutoMapper
{
    /// <summary>
    /// Defines a naming convention strategy
    /// </summary>
    public interface INamingConvention
    {
        /// <summary>
        /// Regular expression on how to tokenize a member
        /// </summary>
        Regex SplittingExpression { get; }
        string SeparatorCharacter { get; }
        string ReplaceValue(Match match);
    }
    public class ExactMatchNamingConvention : INamingConvention
    {
        public static readonly ExactMatchNamingConvention Instance = new ExactMatchNamingConvention();
        public Regex SplittingExpression { get; }
        public string SeparatorCharacter => "";
        public string ReplaceValue(Match match) => match.Value;
    }
    public class PascalCaseNamingConvention : INamingConvention
    {
        private static readonly Regex PascalCase = new Regex(@"(\p{Lu}+(?=$|\p{Lu}[\p{Ll}0-9])|\p{Lu}?[\p{Ll}0-9]+)");
        public static readonly PascalCaseNamingConvention Instance = new PascalCaseNamingConvention();
        public Regex SplittingExpression { get; } = PascalCase;
        public string SeparatorCharacter => string.Empty;
        public string ReplaceValue(Match match) => match.Value[0].ToString().ToUpper() + match.Value.Substring(1);
    }
    public class LowerUnderscoreNamingConvention : INamingConvention
    {
        private static readonly Regex LowerUnderscore = new Regex(@"[\p{Ll}\p{Lu}0-9]+(?=_?)");
        public static readonly LowerUnderscoreNamingConvention Instance = new LowerUnderscoreNamingConvention();
        public Regex SplittingExpression { get; } = LowerUnderscore;
        public string SeparatorCharacter => "_";
        public string ReplaceValue(Match match) => match.Value.ToLower();
    }
}