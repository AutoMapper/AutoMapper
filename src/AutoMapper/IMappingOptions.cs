namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// Options for matching source/destination member types
    /// </summary>
    public interface IMappingOptions
    {
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
        /// Naming convention for source members
        /// </summary>
        INamingConvention SourceMemberNamingConvention { get; }
        /// <summary>
        /// Naming convention for destination members
        /// </summary>
        INamingConvention DestinationMemberNamingConvention { get; }
        /// <summary>
        /// Source member name prefixes to ignore/drop
        /// </summary>
        IEnumerable<string> Prefixes { get; }
        /// <summary>
        /// Source member name postfixes to ignore/drop
        /// </summary>
        IEnumerable<string> Postfixes { get; }

        /// <summary>
        /// Destination member name prefixes to ignore/drop
        /// </summary>
        IEnumerable<string> DestinationPrefixes { get; }

        /// <summary>
        /// Destination member naem prefixes to ignore/drop
        /// </summary>
        IEnumerable<string> DestinationPostfixes { get; }

        /// <summary>
        /// Source/destination member name replacers
        /// </summary>
        IEnumerable<MemberNameReplacer> MemberNameReplacers { get; }

        /// <summary>
        /// Source/destination member aliases
        /// </summary>
        IEnumerable<AliasedMember> Aliases { get; }

        /// <summary>
        /// Allow mapping to constructors that accept arguments
        /// </summary>
        bool ConstructorMappingEnabled { get; }

        /// <summary>
        /// Source extension methods included for search
        /// </summary>
        IEnumerable<MethodInfo> SourceExtensionMethods { get; }
    }
}