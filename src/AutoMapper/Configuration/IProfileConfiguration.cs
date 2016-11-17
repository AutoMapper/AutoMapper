namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Conventions;
    using Mappers;

    /// <summary>
    /// Contains profile-specific configuration
    /// </summary>
    public interface IProfileConfiguration
    {
        IEnumerable<IMemberConfiguration> MemberConfigurations { get; }
        IEnumerable<IConditionalObjectMapper> TypeConfigurations { get; }
        bool? ConstructorMappingEnabled { get; }
        bool? AllowNullDestinationValues { get; }
        bool? AllowNullCollections { get; }
        bool? EnableNullPropagationForQueryMapping { get; }
        bool? CreateMissingTypeMaps { get; }
        IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
        IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> AllPropertyMapActions { get; }

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
        INamingConvention SourceMemberNamingConvention { get; }
        INamingConvention DestinationMemberNamingConvention { get; }
        IEnumerable<ITypeMapConfiguration> TypeMapConfigs { get; }
        IEnumerable<ITypeMapConfiguration> OpenTypeMapConfigs { get; }
    }
}