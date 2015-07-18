namespace AutoMapper
{
    /// <summary>
    /// Source member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    public interface ISourceMemberConfigurationExpression<TSource> : ISourceMemberConfigurationExpression
    {
    }

    /// <summary>
    /// Source member configuration options
    /// </summary>
    public interface ISourceMemberConfigurationExpression
    {
        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();
    }
}