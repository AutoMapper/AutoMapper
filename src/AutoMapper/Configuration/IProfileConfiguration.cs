using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Mappers;

namespace AutoMapper.Configuration
{
    /// <summary>
    /// Contains profile-specific configuration
    /// </summary>
    public interface IProfileConfiguration
    {
        IEnumerable<IMemberConfiguration> MemberConfigurations { get; }
        bool? ConstructorMappingEnabled { get; }
        bool? AllowNullDestinationValues { get; }
        bool? AllowNullCollections { get; }
        bool? EnableNullPropagationForQueryMapping { get; }
        IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
        IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> AllPropertyMapActions { get; }

        /// <summary>
        /// Source extension methods included for search
        /// </summary>
        IEnumerable<MethodInfo> SourceExtensionMethods { get; }

        /// <summary>
        /// Specify which properties should be mapped.
        /// By default only public properties are mapped.
        /// </summary>
        Func<PropertyInfo, bool> ShouldMapProperty { get; }

        /// <summary>
        /// Specify which fields should be mapped.
        /// By default only public fields are mapped.
        /// </summary>
        Func<FieldInfo, bool> ShouldMapField { get; }

        /// <summary>
        /// Specify which methods, of those that are eligible (public, parameterless, and non-static or extension methods), should be mapped.
        /// By default all eligible methods are mapped.
        /// </summary>
        Func<MethodInfo, bool> ShouldMapMethod { get; }


        /// <summary>
        /// Specify which constructors should be considered for the destination objects.
        /// By default all constructors are considered.
        /// </summary>
        Func<ConstructorInfo, bool> ShouldUseConstructor { get; }

        string ProfileName { get; }
        IEnumerable<string> GlobalIgnores { get; }
        INamingConvention SourceMemberNamingConvention { get; }
        INamingConvention DestinationMemberNamingConvention { get; }
        IEnumerable<ITypeMapConfiguration> TypeMapConfigs { get; }
        IEnumerable<ITypeMapConfiguration> OpenTypeMapConfigs { get; }
        IEnumerable<ValueTransformerConfiguration> ValueTransformers { get; }
    }
}
