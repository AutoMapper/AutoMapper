namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;

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
            int count = 0;

            foreach (object item in enumerableValue)
            {
                var typeMap = context.ConfigurationProvider.ResolveTypeMap(item, null, sourceElementType, destKvpType);

                Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceElementType;
                Type targetDestinationType = typeMap != null ? typeMap.DestinationType : destKvpType;

                var newContext = context.CreateElementContext(typeMap, item, targetSourceType, targetDestinationType, count);

                object mappedValue = context.Engine.Map(newContext);
                var keyProperty = mappedValue.GetType().GetProperty("Key");
                object destKey = keyProperty.GetValue(mappedValue, null);

                var valueProperty = mappedValue.GetType().GetProperty("Value");
                object destValue = valueProperty.GetValue(mappedValue, null);

                genericDestDictType.GetMethod("Add").Invoke(destDictionary, new[] { destKey, destValue });

                count++;
            }

            return destDictionary;
        }
    }
}