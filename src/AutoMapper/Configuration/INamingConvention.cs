using System;
using System.Collections.Generic;
namespace AutoMapper
{
    /// <summary>
    /// Defines a naming convention strategy
    /// </summary>
    public interface INamingConvention
    {
        string[] Split(string input);
        string SeparatorCharacter { get; }
    }
    public class ExactMatchNamingConvention : INamingConvention
    {
        public static readonly ExactMatchNamingConvention Instance = new();
        public string[] Split(string _) => Array.Empty<string>();
        public string SeparatorCharacter => null;
    }
    public class PascalCaseNamingConvention : INamingConvention
    {
        public static readonly PascalCaseNamingConvention Instance = new();
        public string SeparatorCharacter => "";
        public string[] Split(string input)
        {
            List<string> result = null;
            int lower = 0;
            for(int index = 1; index < input.Length; index++)
            {
                if (char.IsUpper(input[index]))
                {
                    result ??= new();
                    result.Add(input[lower..index]);
                    lower = index;
                }
            }
            if (result == null)
            {
                return Array.Empty<string>();
            }
            result.Add(input[lower..]);
            return result.ToArray();
        }
    }
    public class LowerUnderscoreNamingConvention : INamingConvention
    {
        public static readonly LowerUnderscoreNamingConvention Instance = new();
        public string SeparatorCharacter => "_";
        public string[] Split(string input) => input.Split('_', StringSplitOptions.RemoveEmptyEntries);
    }
}