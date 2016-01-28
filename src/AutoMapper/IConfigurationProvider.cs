using System.Collections.Generic;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using System;
    using QueryableExtensions;

    public interface IConfigurationProvider
    {
        /// <summary>
        /// Get all configured type maps created
        /// </summary>
        /// <returns>All configured type maps</returns>
        TypeMap[] GetAllTypeMaps();

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
        TypeMap FindTypeMapFor(Type sourceType, Type destinationType);

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured type pair
        /// </summary>
        /// <param name="typePair">Type pair</param>
        /// <returns>Type map configuration</returns>
        TypeMap FindTypeMapFor(TypePair typePair);

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <returns>Type map configuration</returns>
        TypeMap FindTypeMapFor<TSource, TDestination>();

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type, checking the source/destination object types too
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
        TypeMap ResolveTypeMap(object source, object destination, Type sourceType, Type destinationType);

        /// <summary>
        /// Resolve the <see cref="TypeMap"/> for the configured source and destination type, checking parent types
        /// </summary>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
        TypeMap ResolveTypeMap(Type sourceType, Type destinationType);

        /// <summary>
        /// Resolve the <see cref="TypeMap"/> for the configured type pair, checking parent types
        /// </summary>
        /// <param name="typePair">Type pair</param>
        /// <returns>Type map configuration</returns>
        TypeMap ResolveTypeMap(TypePair typePair);

        /// <summary>
        /// Resolve the <see cref="TypeMap"/> for the resolution result and destination type, checking parent types
        /// </summary>
        /// <param name="resolutionResult">Resolution result from the source object</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
        TypeMap ResolveTypeMap(ResolutionResult resolutionResult, Type destinationType);

        /// <summary>
        /// Get named profile configuration
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns></returns>
        IProfileConfiguration GetProfileConfiguration(string profileName);

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
        /// Dry run all type maps in given profile
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        void AssertConfigurationIsValid<TProfile>() where TProfile : Profile, new();

        /// <summary>
        /// Get all configured mappers
        /// </summary>
        /// <returns>List of mappers</returns>
        IEnumerable<IObjectMapper> GetMappers();

        /// <summary>
        /// Get all configured mappers
        /// </summary>
        /// <returns>List of mappers</returns>
        IEnumerable<ITypeMapObjectMapper> GetTypeMapMappers();

        /// <summary>
        /// Factory method to create formatters, resolvers and type converters
        /// </summary>
        Func<Type, object> ServiceCtor { get; }

        /// <summary>
        /// Allow null destination values. If false, destination objects will be created for deep object graphs.
        /// </summary>
        bool AllowNullDestinationValues { get; }

        /// <summary>
        /// Allow null destination collections. If true, null source collections result in null destination collections.
        /// </summary>
        bool AllowNullCollections { get; }

        IExpressionBuilder ExpressionBuilder { get; }
    }
}