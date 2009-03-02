using System;
using System.Collections;

namespace AutoMapper
{
	public interface IConfiguration : IProfileConfiguration
	{
		TypeMap[] GetAllTypeMaps();
		TypeMap FindTypeMapFor(Type sourceType, Type destinationType);
		TypeMap FindTypeMapFor<TSource, TDestination>();
		IFormatterConfiguration GetProfileConfiguration(string profileName);
		void AssertConfigurationIsValid();
		IObjectMapper[] GetMappers();
	}

}