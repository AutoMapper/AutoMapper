namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;

    public class EnumerableToDictionaryMapper : IObjectMapper
    {
        private static readonly Type KvpType = typeof(KeyValuePair<,>);

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsDictionaryType())
                   && (context.SourceType.IsEnumerableType())
                   && (!context.SourceType.IsDictionaryType());
        }

        public object Map(ResolutionContext context)
        {
            var sourceEnumerableValue = (IEnumerable)context.SourceValue ?? new object[0];
            IEnumerable<object> enumerableValue = sourceEnumerableValue.Cast<object>();

            Type sourceElementType = TypeHelper.GetElementType(context.SourceType, sourceEnumerableValue);
            Type genericDestDictType = context.DestinationType.GetDictionaryType();
            Type destKeyType = genericDestDictType.GetTypeInfo().GenericTypeArguments[0];
            Type destValueType = genericDestDictType.GetTypeInfo().GenericTypeArguments[1];
            Type destKvpType = KvpType.MakeGenericType(destKeyType, destValueType);

            object destDictionary = ObjectCreator.CreateDictionary(context.DestinationType, destKeyType, destValueType);

            foreach (object item in enumerableValue)
            {
                object mappedValue = context.Mapper.Map(item, null, sourceElementType, destKvpType, context);
                var keyProperty = mappedValue.GetType().GetProperty("Key");
                object destKey = keyProperty.GetValue(mappedValue, null);

                var valueProperty = mappedValue.GetType().GetProperty("Value");
                object destValue = valueProperty.GetValue(mappedValue, null);

                genericDestDictType.GetMethod("Add").Invoke(destDictionary, new[] { destKey, destValue });
            }

            return destDictionary;
        }
    }
}