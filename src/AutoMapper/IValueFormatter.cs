using System;

namespace AutoMapper
{
    [Obsolete("Formatters should not be used")]
	public interface IValueFormatter
	{
		string FormatValue(ResolutionContext context);
	}

}
