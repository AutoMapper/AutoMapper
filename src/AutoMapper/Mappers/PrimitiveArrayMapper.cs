using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;

    public class PrimitiveArrayMapper : IObjectMapper
    {
        public static TDestElement[] Map<TSourceElement, TDestElement>(TSourceElement[] source, ResolutionContext context)
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }

            if (source != null && typeof(TDestElement).IsAssignableFrom(typeof(TSourceElement)))
            {
                return source as TDestElement[];
            }
            
            var sourceArray = source ?? new TSourceElement[0];

            int sourceLength = sourceArray.Length;
            TDestElement[] destArray = new TDestElement[sourceLength];

            Array.Copy(sourceArray, destArray, sourceLength);

            return destArray;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(PrimitiveArrayMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
            Type destElementType = TypeHelper.GetElementType(context.SourceType);

            return
                MapMethodInfo.MakeGenericMethod(sourceElementType, destElementType)
                    .Invoke(null, new[] { context.SourceValue, context });
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

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            Type sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            Type destElementType = TypeHelper.GetElementType(destExpression.Type);

            return Expression.Call(null,
                MapMethodInfo.MakeGenericMethod(sourceElementType, destElementType), sourceExpression, contextExpression);
        }
    }
}