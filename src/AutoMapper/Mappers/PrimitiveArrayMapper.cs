namespace AutoMapper.Mappers
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class PrimitiveArrayMapper : IObjectMapper
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

            var sourceElementType = context.SourceType.GetNullEnumerableElementType();
            var destElementType = context.DestinationType.GetNullEnumerableElementType();

            var sourceArray = (Array) context.SourceValue ?? ObjectCreator.CreateArray(sourceElementType, 0);

            var sourceLength = sourceArray.Length;
            var destArray = ObjectCreator.CreateArray(destElementType, sourceLength);

            Array.Copy(sourceArray, destArray, sourceLength);

            return destArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsPrimitiveArrayType(Type type)
        {
            if (type.IsArray)
            {
                var elementType = type.GetNullEnumerableElementType();
                //TODO: check this works... I believe it should...
                return elementType.IsPrimitive() || elementType == typeof (string);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return IsPrimitiveArrayType(context.DestinationType)
                   && IsPrimitiveArrayType(context.SourceType)
                   && context.DestinationType.GetElementType() == context.SourceType.GetElementType();
        }
    }
}