using System;

namespace AutoMapper
{
    /// <summary>
    /// Auto map to this destination type from the specified source type.
    /// Discovered during scanning assembly scanning for configuration when calling <see cref="O:AutoMapper.IMapperConfigurationExpression.AddMaps"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public sealed class AutoMapAttribute : Attribute
    {
        public AutoMapAttribute(Type sourceType) 
            => SourceType = sourceType;

        public Type SourceType { get; }
        public bool ReverseMap { get; set; }

        /// <summary>
        /// If set to true, construct the destination object using the service locator.
        /// </summary>
        public bool ConstructUsingServiceLocator { get; set; }

        /// <summary>
        /// For self-referential types, limit recurse depth.
        /// Enables PreserveReferences.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// If set to true, preserve object identity. Useful for circular references.
        /// </summary>
        public bool PreserveReferences { get; set; }

        public void ApplyConfiguration(IMappingExpression mappingExpression)
        {
            if (ReverseMap)
            {
                mappingExpression.ReverseMap();
            }

            if (ConstructUsingServiceLocator)
            {
                mappingExpression.ConstructUsingServiceLocator();
            }

            if (MaxDepth > 0)
            {
                mappingExpression.MaxDepth(MaxDepth);
            }

            if (PreserveReferences)
            {
                mappingExpression.PreserveReferences();
            }
        }
    }
}