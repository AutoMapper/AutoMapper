using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Mappers;

namespace AutoMapper
{
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
        INamingConvention SourceMemberNamingConvention { get; }
        INamingConvention DestinationMemberNamingConvention { get; }
        bool CreateMissingTypeMaps { get; }

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
	}
}