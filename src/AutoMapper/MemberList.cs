namespace AutoMapper
{
    /// <summary>
    /// Member list to check for configuration validation
    /// </summary>
    public enum MemberList
    {
        /// <summary>
        /// Check that all destination members are mapped
        /// </summary>
        Destination = 0,

        /// <summary>
        /// Check that all source members are mapped
        /// </summary>
        Source = 1,

        /// <summary>
        /// Check neither source nor destination members, skipping validation
        /// </summary>
        None = 2
    }
}
