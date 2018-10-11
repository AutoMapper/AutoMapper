namespace AutoMapper
{
    /// <summary>
    /// Source member configuration options
    /// </summary>
    public interface ISourceMemberConfigurationExpression
    {
        /// <summary>
        /// Ignore this member when validating source members, MemberList.Source.
        /// Does not affect validation for the default case, MemberList.Destination.
        /// </summary>
        void DoNotValidate();
    }
}