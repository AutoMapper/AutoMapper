using System;
using System.Collections.Generic;
namespace AutoMapper
{
    using StringChars = ReadOnlyMemory<char>;
    /// <summary>
    /// Defines a naming convention strategy
    /// </summary>
    public interface INamingConvention
    {
        List<StringChars> Split(string input);
        string SeparatorCharacter { get; }
    }
    public class ExactMatchNamingConvention : INamingConvention
    {
        public static readonly ExactMatchNamingConvention Instance = new();
        public List<StringChars> Split(string _) => null;
        public string SeparatorCharacter => null;
    }
    public class PascalCaseNamingConvention : INamingConvention
    {
        public static readonly PascalCaseNamingConvention Instance = new();
        public string SeparatorCharacter => "";
        public List<StringChars> Split(string input)
        {
            List<StringChars> result = null;
            int lower = 0;
            for(int index = 1; index < input.Length; index++)
            {
                if (char.IsUpper(input[index]))
                {
                    result ??= new();
                    result.Add(input.AsMemory(lower, index - lower));
                    lower = index;
                }
            }
            if (result == null)
            {
                return null;
            }
            result.Add(input.AsMemory(lower));
            return result;
        }
    }
    public class LowerUnderscoreNamingConvention : INamingConvention
    {
        public static readonly LowerUnderscoreNamingConvention Instance = new();
        public string SeparatorCharacter => "_";
        public List<StringChars> Split(string input) => new(Array.ConvertAll(input.Split('_', StringSplitOptions.RemoveEmptyEntries), MemoryExtensions.AsMemory));
    }
}