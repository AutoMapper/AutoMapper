using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Mappers
{
	public class EnumerableMapper : EnumerableMapperBase<IList>
	{
		public override bool IsMatch(ResolutionContext context)
		{
            if (context == null) throw new ArgumentNullException("context");

			return (context.DestinationType.IsEnumerableType()) && (context.SourceType.IsEnumerableType());
		}

		protected override void SetElementValue(IList destination, object mappedValue, int index)
		{
            if(destination == null) throw new ArgumentNullException("destination");

			destination.Add(mappedValue);
		}

		protected override void ClearEnumerable(IList enumerable)
		{
            if(enumerable == null) throw new ArgumentNullException("enumerable");

			enumerable.Clear();
		}

		protected override IList CreateDestinationObjectBase(Type destElementType, int sourceLength)
		{
            if(destElementType == null) throw new ArgumentNullException("destElementType");

			return ObjectCreator.CreateList(destElementType);
		}
	}

	public class EnumerableToDictionaryMapper : IObjectMapper
	{
		private static readonly Type KvpType = typeof(KeyValuePair<,>);

		public bool IsMatch(ResolutionContext context)
		{
            if(context == null) throw new ArgumentNullException("context");

			return (context.DestinationType.IsDictionaryType()) 
				&& (context.SourceType.IsEnumerableType())
				&& (!context.SourceType.IsDictionaryType());
		}

		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
            if (context == null) throw new ArgumentNullException("context");
            if (mapper == null) throw new ArgumentNullException("mapper");

			var sourceEnumerableValue = (IEnumerable)context.SourceValue ?? new object[0];
			IEnumerable<object> enumerableValue = sourceEnumerableValue.Cast<object>();

			Type sourceElementType = TypeHelper.GetElementType(context.SourceType, sourceEnumerableValue);
			Type genericDestDictType = context.DestinationType.GetDictionaryType();
			Type destKeyType = genericDestDictType.GetGenericArguments()[0];
			Type destValueType = genericDestDictType.GetGenericArguments()[1];
			Type destKvpType = KvpType.MakeGenericType(destKeyType, destValueType);

			object destDictionary = ObjectCreator.CreateDictionary(context.DestinationType, destKeyType, destValueType);
			int count = 0;

			foreach (object item in enumerableValue)
			{
				var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(item, sourceElementType, destKvpType);

				Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceElementType;
				Type targetDestinationType = typeMap != null ? typeMap.DestinationType : destKvpType;

				var newContext = context.CreateElementContext(typeMap, item, targetSourceType, targetDestinationType, count);

				object mappedValue = mapper.Map(newContext);
				var keyProperty = mappedValue.GetType().GetProperty("Key");
				object destKey = keyProperty.GetValue(mappedValue, null);
				
				var valueProperty = mappedValue.GetType().GetProperty("Value");
				object destValue = valueProperty.GetValue(mappedValue, null);

				genericDestDictType.GetMethod("Add").Invoke(destDictionary, new[] {destKey, destValue});

				count++;
			}

			return destDictionary;
		}
	}
}