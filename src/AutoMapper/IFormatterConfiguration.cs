using System;
using System.Collections.Generic;

namespace AutoMapper
{
	public interface IFormatterConfiguration : IProfileConfiguration
	{
		IValueFormatter[] GetFormatters();
		IDictionary<Type, IFormatterConfiguration> GetTypeSpecificFormatters();
		Type[] GetFormatterTypesToSkip();
	}

	public interface IProfileConfiguration
	{
		bool MapNullSourceValuesAsNull { get; }
	}
}