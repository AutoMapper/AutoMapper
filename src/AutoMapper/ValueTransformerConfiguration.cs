using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper
{
    public readonly struct ValueTransformerConfiguration
    {
        public ValueTransformerConfiguration(Type valueType, LambdaExpression transformerExpression)
        {
            ValueType = valueType;
            TransformerExpression = transformerExpression;
        }

        public Type ValueType { get; }
        public LambdaExpression TransformerExpression { get; }

        public bool IsMatch(IMemberMap memberMap) 
            => ValueType.IsAssignableFrom(memberMap.SourceType) && memberMap.DestinationType.IsAssignableFrom(ValueType);
    }

    public static class ValueTransformerConfigurationExtensions
    {
        /// <summary>
        /// Apply a transformation function after any resolved destination member value with the given type
        /// </summary>
        /// <typeparam name="TValue">Value type to match and transform</typeparam>
        /// <param name="valueTransformers">Value transformer list</param>
        /// <param name="transformer">Transformation expression</param>
        public static void Add<TValue>(this IList<ValueTransformerConfiguration> valueTransformers,
            Expression<Func<TValue, TValue>> transformer)
        {
            var config = new ValueTransformerConfiguration(typeof(TValue), transformer);

            valueTransformers.Add(config);
        }
    }
}