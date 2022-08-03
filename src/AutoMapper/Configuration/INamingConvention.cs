using System;
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
        public static readonly ExactMatchNamingConvention Instance = new();
        public Regex SplittingExpression { get; }
        public string SeparatorCharacter => "";
        public string ReplaceValue(Match match) => match.Value;
    }
    public class PascalCaseNamingConvention : INamingConvention
    {
        private static readonly Regex PascalCase = new(@"(\p{Lu}+(?=$|\p{Lu}[\p{Ll}0-9])|\p{Lu}?[\p{Ll}0-9]+)");
        public static readonly PascalCaseNamingConvention Instance = new();
        public Regex SplittingExpression { get; } = PascalCase;
        public string SeparatorCharacter => string.Empty;
        public string ReplaceValue(Match match)
        {
            var source = match.Value;
            return string.Create(source.Length, source, static (buffer, state) =>
            {
                buffer[0] = char.ToUpper(state[0]);
                state.AsSpan(1).CopyTo(buffer[1..]);
            });
        }
    }
    public class LowerUnderscoreNamingConvention : INamingConvention
    {
        private static readonly Regex LowerUnderscore = new(@"[\p{Ll}\p{Lu}0-9]+(?=_?)");
        public static readonly LowerUnderscoreNamingConvention Instance = new();
        public Regex SplittingExpression { get; } = LowerUnderscore;
        public string SeparatorCharacter => "_";
        public string ReplaceValue(Match match) => match.Value.ToLower();
    }
}