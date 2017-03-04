using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using System;
    using QueryableExtensions;

    public interface IConfigurationProvider
    {
        void Validate(ValidationContext context);

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
        /// Factory method to create formatters, resolvers and type converters
        /// </summary>
        Func<Type, object> ServiceCtor { get; }

        /// <summary>
        /// Allows to enable null-value propagation for query mapping. 
        /// <remarks>Some providers (such as EntityFrameworkQueryVisitor) do not work with this feature enabled!</remarks>
        /// </summary>
        bool EnableNullPropagationForQueryMapping { get; }

        IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Create a mapper instance based on this configuration. Mapper instances are lightweight and can be created as needed.
        /// </summary>
        /// <returns>The mapper instance</returns>
        IMapper CreateMapper();

        /// <summary>
        /// Create a mapper instance with the specified service constructor to be used for resolvers and type converters.
        /// </summary>
        /// <param name="serviceCtor">Service factory to create services</param>
        /// <returns>The mapper instance</returns>
        IMapper CreateMapper(Func<Type, object> serviceCtor);

        Func<TSource, TDestination, ResolutionContext, TDestination> GetMapperFunc<TSource, TDestination>(TypePair types);

        /// <summary>
        /// Compile all underlying mapping expressions to cached delegates.
        /// Use if you want AutoMapper to compile all mappings up front instead of deferring expression compilation for each first map.
        /// </summary>
        void CompileMappings();

        Delegate GetMapperFunc(MapRequest request);

        Func<object, object, ResolutionContext, object> GetUntypedMapperFunc(MapRequest mapRequest);

        /// <summary>
        /// Builds the execution plan used to map the source to destination.
        /// Useful to understand what exactly is happening during mapping.
        /// See <a href="https://github.com/AutoMapper/AutoMapper/wiki/Understanding-your-mapping">the wiki</a> for details.
        /// </summary>
        /// <param name="sourceType">the runtime type of the source object</param>
        /// <param name="destinationType">the runtime type of the destination object</param>
        /// <returns>the execution plan</returns>
        LambdaExpression BuildExecutionPlan(Type sourceType, Type destinationType);

        /// <summary>
        /// Builds the execution plan used to map the source to destination.
        /// Useful to understand what exactly is happening during mapping.
        /// See <a href="https://github.com/AutoMapper/AutoMapper/wiki/Understanding-your-mapping">the wiki</a> for details.
        /// </summary>
        /// <param name="mapRequest">The source/destination map request</param>
        /// <returns>the execution plan</returns>
        LambdaExpression BuildExecutionPlan(MapRequest mapRequest);
    }
}