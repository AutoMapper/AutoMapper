using System;

namespace AutoMapper
{
    using System.Reflection;

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
        /// <summary>
        /// Get all configured type maps created
        /// </summary>
        /// <returns>All configured type maps</returns>
		TypeMap[] GetAllTypeMaps();

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type, checking the source/destination object types too
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
		TypeMap FindTypeMapFor(object source, object destination, Type sourceType, Type destinationType);

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
		TypeMap FindTypeMapFor(Type sourceType, Type destinationType);

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the resolution result and destination type
        /// </summary>
        /// <param name="resolutionResult">Resolution result from the source object</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
		TypeMap FindTypeMapFor(ResolutionResult resolutionResult, Type destinationType);

        /// <summary>
        /// Get named profile configuration
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns></returns>
		IFormatterConfiguration GetProfileConfiguration(string profileName);


        /// <summary>
        /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
        /// </summary>
		void AssertConfigurationIsValid();

        /// <summary>
        /// Dry run single type map
        /// </summary>
        /// <param name="typeMap">Type map to check</param>
		void AssertConfigurationIsValid(TypeMap typeMap);

        /// <summary>
        /// Dry run all type maps in given profile
        /// </summary>
        /// <param name="profileName">Profile name of type maps to test</param>
		void AssertConfigurationIsValid(string profileName);

        /// <summary>
        /// Get all configured mappers
        /// </summary>
        /// <returns>List of mappers</returns>
		IObjectMapper[] GetMappers();

        /// <summary>
        /// Creates a <see cref="TypeMap"/> based on a source and destination type
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <returns>Type map configuration</returns>
		TypeMap CreateTypeMap(Type sourceType, Type destinationType);

	    TypeMap CreateTypeMap(Type source, Type destination, string profileName, MemberList memberList);

        /// <summary>
        /// Fired each time a type map is created
        /// </summary>
		event EventHandler<TypeMapCreatedEventArgs> TypeMapCreated;

        /// <summary>
        /// Factory method to create formatters, resolvers and type converters
        /// </summary>
	    Func<Type, object> ServiceCtor { get; }
	}

}
