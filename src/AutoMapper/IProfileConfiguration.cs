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
        IList<IMemberConfiguration> MemberConfigurations { get; }
        IList<IConditionalObjectMapper> TypeConfigurations { get; }
        IConditionalObjectMapper AddConditionalObjectMapper(string profile = ConfigurationStore.DefaultProfileName);
        bool ConstructorMappingEnabled { get; set; }
        bool DataReaderMapperYieldReturnEnabled { get; set; }

        /// <summary>
        /// Source extension methods included for search
        /// </summary>
        IEnumerable<MethodInfo> SourceExtensionMethods { get; }

        void IncludeSourceExtensionMethods(Assembly assembly);
        /// <summary>
        /// Specify which properties should be mapped.
        /// By default only public properties are mapped.
        /// </summary>
        Func<PropertyInfo, bool> ShouldMapProperty { get; set; }

        /// <summary>
        /// Specify which fields should be mapped.
        /// By default only public fields are mapped.
        /// </summary>
        Func<FieldInfo, bool> ShouldMapField { get; set; }
    }
}
