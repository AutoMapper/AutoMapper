using System;

namespace AutoMapper
{
	public class TypeMapCreatedEventArgs : EventArgs
	{
		public TypeMap TypeMap { get; private set; }

		public TypeMapCreatedEventArgs(TypeMap typeMap)
		{
			TypeMap = typeMap;
		}

	}
	public interface IConfigurationProvider : IProfileConfiguration
	{
		TypeMap[] GetAllTypeMaps();
		TypeMap FindTypeMapFor(object source, Type sourceType, Type destinationType);
		IFormatterConfiguration GetProfileConfiguration(string profileName);
		void AssertConfigurationIsValid();
		void AssertConfigurationIsValid(TypeMap typeMap);
		IObjectMapper[] GetMappers();
		TypeMap CreateTypeMap(Type sourceType, Type destinationType);

		event EventHandler<TypeMapCreatedEventArgs> TypeMapCreated;
	}

}