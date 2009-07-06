using System;

namespace AutoMapper
{
	public interface IConfigurationProvider : IProfileConfiguration
	{
		TypeMap[] GetAllTypeMaps();
		TypeMap FindTypeMapFor(object source, Type sourceType, Type destinationType);
		IFormatterConfiguration GetProfileConfiguration(string profileName);
		void AssertConfigurationIsValid();
		void AssertConfigurationIsValid(TypeMap typeMap);
		IObjectMapper[] GetMappers();
		TypeMap CreateTypeMap(Type sourceType, Type destinationType);
	}

}