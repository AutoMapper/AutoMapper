using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using Configuration.Conventions;

    /// <summary>
    /// Contains profile-specific configuration
    /// </summary>
    public interface IProfileConfiguration
    {
        IEnumerable<IMemberConfiguration> MemberConfigurations { get; }
        IEnumerable<IConditionalObjectMapper> TypeConfigurations { get; }
        bool ConstructorMappingEnabled { get; }
        bool AllowNullDestinationValues { get; }
        bool AllowNullCollections { get; }
        bool EnableNullPropagationForQueryMapping { get; }
        bool CreateMissingTypeMaps { get; }
        IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
        IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> AllPropertyMapActions { get; }

        IMemberConfiguration DefaultMemberConfig { get; }
        /// <summary>
        /// Source extension methods included for search
        /// </summary>
        IEnumerable<MethodInfo> SourceExtensionMethods { get; }

        /// <summary>
        /// Specify which properties should be mapped.
        /// By default only public properties are mapped.e
        /// </summary>
        Func<PropertyInfo, bool> ShouldMapProperty { get; }

        /// <summary>
        /// Specify which fields should be mapped.
        /// By default only public fields are mapped.
        /// </summary>
        Func<FieldInfo, bool> ShouldMapField { get; }

        string ProfileName { get; }
        IEnumerable<string> GlobalIgnores { get; }
        IEnumerable<string> Prefixes { get; }
        IEnumerable<string> Postfixes { get; }

        /// <summary>
        /// Registers all defined type maps
        /// </summary>
        /// <param name="typeMapRegistry">Type map registry</param>
        void Register(TypeMapRegistry typeMapRegistry);

        /// <summary>
        /// Configured all defined type maps
        /// </summary>
        /// <param name="typeMapRegistry">Type map registry</param>
        void Configure(TypeMapRegistry typeMapRegistry);

        /// <summary>
        /// Created and configures a type map based on conventions if matching
        /// </summary>
        /// <param name="typeMapRegistry">Type map registry</param>
        /// <param name="conventionTypes">Types to match</param>
        /// <returns>Configured type map, or null if not a match</returns>
        TypeMap ConfigureConventionTypeMap(TypeMapRegistry typeMapRegistry, TypePair conventionTypes);

        /// <summary>
        /// Creates and configures a closed generic type map based on conventions if matching
        /// </summary>
        /// <param name="typeMapRegistry">Type map registry</param>
        /// <param name="closedTypes">Closed types to create</param>
        /// <param name="requestedTypes">The requested types</param>
        /// <returns>Configured type map, or null if not a match</returns>
        TypeMap ConfigureClosedGenericTypeMap(TypeMapRegistry typeMapRegistry, TypePair closedTypes, TypePair requestedTypes);

        TypeDetails CreateTypeDetails(Type type);
    }
}