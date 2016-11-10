namespace AutoMapper
{
    /// <summary>
    /// Extension point to provide custom resolution for a destination value
    /// </summary>
    public interface IValueResolver<in TSource, in TDestination, TDestMember>
    {
        /// <summary>
        /// Implementors use source object to provide a destination object.
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object, if exists</param>
        /// <param name="destMember">Destination member</param>
        /// <param name="context">The context of the mapping</param>
        /// <returns>Result, typically build from the source resolution result</returns>
        TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
    }

    /// <summary>
    /// Extension point to provide custom resolution for a destination value
    /// </summary>
    public interface IMemberValueResolver<in TSource, in TDestination, in TSourceMember, TDestMember>
    {
        /// <summary>
        /// Implementors use source object to provide a destination object.
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object, if exists</param>
        /// <param name="sourceMember">Source member</param>
        /// <param name="destMember">Destination member</param>
        /// <param name="context">The context of the mapping</param>
        /// <returns>Result, typically build from the source resolution result</returns>
        TDestMember Resolve(TSource source, TDestination destination, TSourceMember sourceMember, TDestMember destMember, ResolutionContext context);
    }
}
