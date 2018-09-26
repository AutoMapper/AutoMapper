namespace AutoMapper
{
    /// <summary>
    /// Converts a source member value to a destination member value
    /// </summary>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    /// <typeparam name="TDestinationMember">Destination member type</typeparam>
    public interface IValueConverter<in TSourceMember, out TDestinationMember>
    {
        /// <summary>
        /// Perform conversion from source member value to destination member value
        /// </summary>
        /// <param name="sourceMember">Source member object</param>
        /// <param name="context">Resolution context</param>
        /// <returns>Destination member value</returns>
        TDestinationMember Convert(TSourceMember sourceMember, ResolutionContext context);
    }
}