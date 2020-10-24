using System.Text.RegularExpressions;

namespace AutoMapper
{
    public class PascalCaseNamingConvention : INamingConvention
    {
        private static readonly Regex PascalCase = new Regex(@"(\p{Lu}+(?=$|\p{Lu}[\p{Ll}0-9])|\p{Lu}?[\p{Ll}0-9]+)");
        public static readonly PascalCaseNamingConvention Instance = new PascalCaseNamingConvention();
        public Regex SplittingExpression { get; } = PascalCase;
        public string SeparatorCharacter => string.Empty;
        public string ReplaceValue(Match match) => match.Value[0].ToString().ToUpper() + match.Value.Substring(1);
    }
}