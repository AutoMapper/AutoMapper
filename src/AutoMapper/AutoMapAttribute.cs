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
        public bool ConstructUsingServiceLocator { get; set; }

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
        }
    }
}