using System;
using System.Collections.Generic;

namespace AutoMapper
{
	public interface IFormatterConfiguration
	{
		IValueFormatter[] GetFormatters();
		IDictionary<Type, IFormatterConfiguration> GetTypeSpecificFormatters();
		Type[] GetFormatterTypesToSkip();
	}
}