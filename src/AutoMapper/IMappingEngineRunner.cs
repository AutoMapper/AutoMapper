using System;

namespace AutoMapper
{
	public interface IMappingEngineRunner
	{
		object Map(ResolutionContext context);
		object CreateObject(ResolutionContext context);
		string FormatValue(ResolutionContext context);
		IConfigurationProvider ConfigurationProvider { get; }
	}
}