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
}