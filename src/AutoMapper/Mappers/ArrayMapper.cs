namespace AutoMapper.Mappers
{
    using System;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class ArrayMapper : EnumerableMapperBase<Array>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool IsMatch(ResolutionContext context)
        {
            return context.DestinationType.IsArray
                && context.SourceType.IsEnumerableType();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerable"></param>
        protected override void ClearEnumerable(Array enumerable)
        {
            // no op
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="mappedValue"></param>
        /// <param name="index"></param>
        protected override void SetElementValue(Array destination, object mappedValue, int index)
        {
            destination.SetValue(mappedValue, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destElementType"></param>
        /// <param name="sourceLength"></param>
        /// <returns></returns>
        protected override Array CreateDestinationObjectBase(Type destElementType, int sourceLength)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destElementType"></param>
        /// <param name="sourceLength"></param>
        /// <returns></returns>
        protected override object GetOrCreateDestinationObject(ResolutionContext context,
            Type destElementType, int sourceLength)
        {
            return ObjectCreator.CreateArray(destElementType, sourceLength);
        }
    }
}
