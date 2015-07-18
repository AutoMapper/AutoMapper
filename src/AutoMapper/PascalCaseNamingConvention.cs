namespace AutoMapper
{
    using System.Text.RegularExpressions;

    public class PascalCaseNamingConvention : INamingConvention
    {
        public Regex SplittingExpression { get; } = new Regex(@"(\p{Lu}+(?=$|\p{Lu}[\p{Ll}0-9])|\p{Lu}?[\p{Ll}0-9]+)");

        public string SeparatorCharacter => string.Empty;
        public string ReplaceValue(Match match)
        {
            return match.Value[0].ToString().ToUpper() + match.Value.Substring(1);
        }
    }
}