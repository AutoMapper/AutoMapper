namespace AutoMapper
{
    /// <summary>
    /// Contains profile-specific configuration
    /// </summary>
	public interface IProfileConfiguration
	{
        /// <summary>
        /// Indicates that null source values should be mapped as null
        /// </summary>
		bool MapNullSourceValuesAsNull { get; }

        /// <summary>
        /// Indicates that null source collections should be mapped as null
        /// </summary>
		bool MapNullSourceCollectionsAsNull { get; }
	}
}
