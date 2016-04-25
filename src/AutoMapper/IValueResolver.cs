namespace AutoMapper
{
    /// <summary>
    /// Extension point to provide custom resolution for a destination value
    /// </summary>
	public interface IValueResolver<in TSource, out TMember>
    {
        /// <summary>
        /// Implementors use source object to provide a destination object.
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="context">The context of the mapping</param>
        /// <returns>Result, typically build from the source resolution result</returns>
        TMember Resolve(TSource source, ResolutionContext context);
    }
}
