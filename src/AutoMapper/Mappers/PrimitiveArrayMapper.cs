namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using Internal;

    public class PrimitiveArrayMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            if (context.IsSourceValueNull && context.Engine.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }

            if (!context.IsSourceValueNull && context.DestinationType.IsAssignableFrom(context.SourceType))
            {
                return context.SourceValue;
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

        public bool IsMatch(TypePair context)
        {
            return IsPrimitiveArrayType(context.DestinationType) &&
                   IsPrimitiveArrayType(context.SourceType) &&
                   (TypeHelper.GetElementType(context.DestinationType)
                       .Equals(TypeHelper.GetElementType(context.SourceType)));
        }
    }
}