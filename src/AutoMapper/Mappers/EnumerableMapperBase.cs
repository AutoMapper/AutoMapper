namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEnumerable"></typeparam>
    public abstract class EnumerableMapperBase<TEnumerable> : IObjectMapper
        where TEnumerable : IEnumerable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var runner = context.MapperContext.Runner;
            if (context.IsSourceValueNull && runner.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }

            ICollection<object> enumerableValue = ((IEnumerable) context.SourceValue ?? new object[0])
                .Cast<object>()
                .ToList();

            var sourceElementType = context.SourceType.GetElementType(enumerableValue);
            var destElementType = context.DestinationType.GetNullEnumerableElementType();

            var sourceLength = enumerableValue.Count;
            var destination = GetOrCreateDestinationObject(context, destElementType, sourceLength);
            var enumerable = GetEnumerableFor(destination);

            ClearEnumerable(enumerable);

            var i = 0;
            foreach (var item in enumerableValue)
            {
                var newContext = context.CreateElementContext(null, item, sourceElementType, destElementType, i);
                var elementResolutionResult = new ResolutionResult(newContext);

                var typeMap = context.MapperContext.ConfigurationProvider.ResolveTypeMap(elementResolutionResult, destElementType);

                var targetSourceType = typeMap != null ? typeMap.SourceType : sourceElementType;
                var targetDestinationType = typeMap != null ? typeMap.DestinationType : destElementType;

                newContext = context.CreateElementContext(typeMap, item, targetSourceType, targetDestinationType, i);

                var mappedValue = runner.Map(newContext);

                SetElementValue(enumerable, mappedValue, i);

                i++;
            }

            return destination;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destElementType"></param>
        /// <param name="sourceLength"></param>
        /// <returns></returns>
        protected virtual object GetOrCreateDestinationObject(ResolutionContext context,
            Type destElementType, int sourceLength)
        {
            var runner = context.MapperContext.Runner;

            if (context.DestinationValue != null)
            {
                // If the source is not an array, assume we can add to it...
                if (!(context.DestinationValue is Array))
                    return context.DestinationValue;

                // If the source is an array, ensure that we have enough room...
                var array = (Array) context.DestinationValue;

                if (array.Length >= sourceLength)
                    return context.DestinationValue;
            }

            return CreateDestinationObject(context, destElementType, sourceLength, runner);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected virtual TEnumerable GetEnumerableFor(object destination)
        {
            return (TEnumerable) destination;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerable"></param>
        protected virtual void ClearEnumerable(TEnumerable enumerable)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationElementType"></param>
        /// <param name="count"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        protected virtual object CreateDestinationObject(ResolutionContext context, Type destinationElementType,
            int count, IMappingEngineRunner mapper)
        {
            var destinationType = context.DestinationType;

            if (!destinationType.IsInterface() && !destinationType.IsArray)
            {
                return mapper.CreateObject(context);
            }
            return CreateDestinationObjectBase(destinationElementType, count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract bool IsMatch(ResolutionContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="mappedValue"></param>
        /// <param name="index"></param>
        protected abstract void SetElementValue(TEnumerable destination, object mappedValue, int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destElementType"></param>
        /// <param name="sourceLength"></param>
        /// <returns></returns>
        protected abstract TEnumerable CreateDestinationObjectBase(Type destElementType, int sourceLength);
    }
}