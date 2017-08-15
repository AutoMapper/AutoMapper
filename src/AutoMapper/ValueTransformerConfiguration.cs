using System;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper
{
    public class ValueTransformConfiguration : IValueTransformConfiguration
    {
        public ValueTransformConfiguration(Type valueType, LambdaExpression transformerExpression)
        {
            ValueType = valueType;
            TransformerExpression = transformerExpression;
        }

        private Type ValueType { get; }
        private LambdaExpression TransformerExpression { get; }

        public bool IsMatch(PropertyMap propertyMap)
        {
            return ValueType.IsAssignableFrom(propertyMap.DestinationPropertyType);
        }

        public Expression Visit(Expression expression, PropertyMap propertyMap)
        {
            return TransformerExpression.ReplaceParameters(expression.ToType(ValueType)).ToType(propertyMap.DestinationPropertyType);
        }
    }

    public static class ValueTransformerConfigurationExtensions
    {
        /// <summary>
        /// Apply a transformation function after any resolved destination member value with the given type
        /// </summary>
        /// <typeparam name="TValue">Value type to match and transform</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingExpression"></param>
        /// <param name="transformer">Transformation expression</param>
        /// <returns>Itself</returns>
        public static T ApplyTransform<T, TValue>(this T mappingExpression, Expression<Func<TValue, TValue>> transformer)
            where T : IMappingExpressionBase
        {
            mappingExpression.TypeMapActions.Add(tm =>
            {
                var config = new ValueTransformConfiguration(typeof(TValue), transformer);

                tm.AddValueTransformation(config);
            });

            return mappingExpression;
        }

        /// <summary>
        /// Apply a transformation function after any resolved destination member value with the given type
        /// </summary>
        /// <typeparam name="TValue">Value type to match and transform</typeparam>
        /// <param name="profileExpression"></param>
        /// <param name="transformer">Transformation expression</param>
        public static void ApplyTransform<TValue>(this IProfileExpression profileExpression, Expression<Func<TValue, TValue>> transformer)
        {
            var config = new ValueTransformConfiguration(typeof(TValue), transformer);

            profileExpression.ValueTransformers.Add(config);
        }

        /// <summary>
        /// Apply a transformation function after any resolved destination member value with the given type
        /// </summary>
        /// <typeparam name="TValue">Value type to match and transform</typeparam>
        /// <param name="self"></param>
        /// <param name="transformer">Transformation expression</param>
        public static void ApplyTransform<TValue>(this IMemberConfigurationExpressionBase self, Expression<Func<TValue, TValue>> transformer)
        {
            self.PropertyMapActions.Add(pm =>
            {
                var config = new ValueTransformConfiguration(typeof(TValue), transformer);

                pm.AddValueTransformation(config);
            });
        }
    }
}