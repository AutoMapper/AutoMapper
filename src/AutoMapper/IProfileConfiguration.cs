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
        /// <summary>
        /// Indicates that null source values should be mapped as null
        /// </summary>
		bool MapNullSourceValuesAsNull { get; set; }

        /// <summary>
        /// Indicates that null source collections should be mapped as null
        /// </summary>
		bool MapNullSourceCollectionsAsNull { get; set; }

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
