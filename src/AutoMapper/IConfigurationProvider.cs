using System;

namespace AutoMapper
{
	public interface IConfigurationProvider : IProfileConfiguration
	{
		TypeMap[] GetAllTypeMaps();
		TypeMap FindTypeMapFor(Type sourceType, Type destinationType);
		TypeMap FindTypeMapFor<TSource, TDestination>();
		IFormatterConfiguration GetProfileConfiguration(string profileName);
		void AssertConfigurationIsValid();
		void AssertConfigurationIsValid(TypeMap typeMap);
		IObjectMapper[] GetMappers();
		TypeMap CreateTypeMap(Type sourceType, Type destinationType);
	}

}