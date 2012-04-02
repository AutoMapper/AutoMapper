using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoMapper
{
	public interface INamingConvention
	{
		Regex SplittingExpression { get; }
		string SeparatorCharacter { get; }
	}

	public interface IMappingOptions
	{
        INamingConvention SourceMemberNamingConvention { get; set; }
        INamingConvention DestinationMemberNamingConvention { get; set; }
	    IEnumerable<string> Prefixes { get; }
	    IEnumerable<string> Postfixes { get; }
	    IEnumerable<string> DestinationPrefixes { get; }
	    IEnumerable<string> DestinationPostfixes { get; }
	    IEnumerable<AliasedMember> Aliases { get; }
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