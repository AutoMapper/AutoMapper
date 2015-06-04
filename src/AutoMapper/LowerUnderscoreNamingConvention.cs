namespace AutoMapper
{
    using System.Text.RegularExpressions;

    public class LowerUnderscoreNamingConvention : INamingConvention
    {
        public Regex SplittingExpression { get; } = new Regex(@"[\p{Ll}0-9]+(?=_?)");

        public string SeparatorCharacter => "_";
    }
}