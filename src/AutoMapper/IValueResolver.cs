namespace AutoMapper
{
    /// <summary>
    /// Extension point to provide custom resolution for a destination value
    /// </summary>
	public interface IValueResolver
	{
        /// <summary>
        /// Implementors use source resolution result to provide a destination resolution result.
        /// Use the <see cref="ValueResolver{TSource, TDestination}"/> class for a type-safe version.
        /// </summary>
        /// <param name="source">Source resolution result</param>
        /// <returns>Result, typically build from the source resolution result</returns>
		ResolutionResult Resolve(ResolutionResult source);
	}
}
