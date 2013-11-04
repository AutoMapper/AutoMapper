using System.Collections.Generic;
using System.Reflection;
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

        /// <summary>
        /// Character to separate on
        /// </summary>
		string SeparatorCharacter { get; }
	}

    /// <summary>
    /// Options for matching source/destination member types
    /// </summary>
	public interface IMappingOptions
	{
        /// <summary>
        /// Naming convention for source members
        /// </summary>
        INamingConvention SourceMemberNamingConvention { get; set; }
        /// <summary>
        /// Naming convention for destination members
        /// </summary>
        INamingConvention DestinationMemberNamingConvention { get; set; }
        /// <summary>
        /// Source member name prefixes to ignore/drop
        /// </summary>
	    IEnumerable<string> Prefixes { get; }
        /// <summary>
        /// Source member name postfixes to ignore/drop
        /// </summary>
	    IEnumerable<string> Postfixes { get; }

        /// <summary>
        /// Destination member name prefixes to ignore/drop
        /// </summary>
	    IEnumerable<string> DestinationPrefixes { get; }

        /// <summary>
        /// Destination member naem prefixes to ignore/drop
        /// </summary>
	    IEnumerable<string> DestinationPostfixes { get; }

        /// <summary>
        /// Source/destination member aliases
        /// </summary>
	    IEnumerable<AliasedMember> Aliases { get; }

        /// <summary>
        /// Allow mapping to constructors that accept arguments
        /// </summary>
	    bool ConstructorMappingEnabled { get; }

        /// <summary>
        /// For mapping via data readers, enable lazy returning of values instead of immediate evalaution
        /// </summary>
	    bool DataReaderMapperYieldReturnEnabled { get; }

        /// <summary>
        /// Assemblies to search for extension methods
        /// </summary>
	    IEnumerable<Assembly> SourceExtensionMethodSearch { get; set; }
	}

	public class PascalCaseNamingConvention : INamingConvention
	{
        private readonly Regex _splittingExpression = new Regex(@"(\p{Lu}+(?=$|\p{Lu}[\p{Ll}0-9])|\p{Lu}?[\p{Ll}0-9]+)");

		public Regex SplittingExpression
		{
			get { return _splittingExpression; }
		}

		public string SeparatorCharacter
		{
			get { return string.Empty; }
		}
	}

	public class LowerUnderscoreNamingConvention : INamingConvention
	{
		private readonly Regex _splittingExpression = new Regex(@"[\p{Ll}0-9]+(?=_?)");

		public Regex SplittingExpression
		{
			get { return _splittingExpression; }
		}

		public string SeparatorCharacter
		{
			get { return "_"; }
		}
	}
}