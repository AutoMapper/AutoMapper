namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;

    public class PrimitiveArrayMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            if (context.IsSourceValueNull && mapper.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }

            Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
            Type destElementType = TypeHelper.GetElementType(context.DestinationType);

            Array sourceArray = (Array) context.SourceValue ?? ObjectCreator.CreateArray(sourceElementType, 0);

            int sourceLength = sourceArray.Length;
            Array destArray = ObjectCreator.CreateArray(destElementType, sourceLength);

            Array.Copy(sourceArray, destArray, sourceLength);

            return destArray;
        }

        private bool IsPrimitiveArrayType(Type type)
        {
            if (type.IsArray)
            {
                Type elementType = TypeHelper.GetElementType(type);
                return elementType.IsPrimitive() || elementType.Equals(typeof (string));
            }

            return false;
        }

        public bool IsMatch(ResolutionContext context)
        {
            return IsPrimitiveArrayType(context.DestinationType) &&
                   IsPrimitiveArrayType(context.SourceType) &&
                   (TypeHelper.GetElementType(context.DestinationType)
                       .Equals(TypeHelper.GetElementType(context.SourceType)));
        }
    }
}