using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoMapper
{
	public interface IFormatterConfiguration : IProfileConfiguration
	{
		IValueFormatter[] GetFormatters();
		IDictionary<Type, IFormatterConfiguration> GetTypeSpecificFormatters();
		Type[] GetFormatterTypesToSkip();
	    IEnumerable<IValueFormatter> GetFormattersToApply(ResolutionContext context);
	}

	public interface IProfileConfiguration
	{
		bool MapNullSourceValuesAsNull { get; }
		bool MapNullSourceCollectionsAsNull { get; }
	}
}