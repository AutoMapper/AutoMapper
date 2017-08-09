using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    public class ValueTransformerConfiguration
    {
        public ValueTransformerConfiguration(Type valueType, LambdaExpression transformerExpression)
        {
            ValueType = valueType;
            TransformerExpression = transformerExpression;
        }

        public Type ValueType { get; }
        public LambdaExpression TransformerExpression { get; }

        public bool IsMatch(PropertyMap propertyMap)
        {
            return ValueType.IsAssignableFrom(propertyMap.DestinationPropertyType);
        }
    }
}