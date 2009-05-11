using System;

namespace AutoMapper
{
	public interface IMappingEngineRunner
	{
		object Map(ResolutionContext context);
		object CreateObject(Type type);
		string FormatValue(ResolutionContext context);
		IConfigurationProvider ConfigurationProvider { get; }
	}
}