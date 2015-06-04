namespace AutoMapper
{
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// Options for matching source/destination member types
    /// </summary>
    public interface IMappingOptions
    {
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
        /// For mapping via data readers, enable lazy returning of values instead of immediate evalaution
        /// </summary>
        bool DataReaderMapperYieldReturnEnabled { get; }

        /// <summary>
        /// Source extension methods included for search
        /// </summary>
        IEnumerable<MethodInfo> SourceExtensionMethods { get; }
    }
}