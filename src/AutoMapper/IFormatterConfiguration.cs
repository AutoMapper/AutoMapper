namespace AutoMapper
{
    using System;
    using System.Collections.Generic;

    [Obsolete("Formatters should not be used")]
	public interface IFormatterConfiguration : IProfileConfiguration
	{
        [Obsolete("Formatters should not be used")]
		IValueFormatter[] GetFormatters();

        [Obsolete("Formatters should not be used")]
        IDictionary<Type, IFormatterConfiguration> GetTypeSpecificFormatters();

        [Obsolete("Formatters should not be used")]
		Type[] GetFormatterTypesToSkip();

        [Obsolete("Formatters should not be used")]
	    IEnumerable<IValueFormatter> GetFormattersToApply(ResolutionContext context);
	}

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
