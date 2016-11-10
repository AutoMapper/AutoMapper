using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Linq;
    using Configuration;

    public class MultidimensionalArrayMapper : IObjectMapper
    {
        static MultidimensionalArrayFiller filler;

        public static Array Map<TDestination, TSource, TSourceElement>(TSource source, ResolutionContext context)
            where TSource : IEnumerable
        {
            if (source == null && context.ConfigurationProvider.Configuration.AllowNullCollections)
                return null;

            var destElementType = TypeHelper.GetElementType(typeof(TDestination));

            if (source != null && typeof(TDestination).IsAssignableFrom(typeof(TSource)))
            {
                var elementTypeMap = context.ConfigurationProvider.ResolveTypeMap(typeof(TSourceElement), destElementType);
                if (elementTypeMap == null)
                    return source as Array;
            }

            IEnumerable sourceList = source;
            if (sourceList == null)
                sourceList = typeof(TSource).GetTypeInfo().IsInterface ?
                new List<TSourceElement>() :
                (IEnumerable<TSourceElement>)(context.ConfigurationProvider.Configuration.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof(TSource))
                : ObjectCreator.CreateObject(typeof(TSource)));

            var sourceLength = sourceList.OfType<object>().Count();
            var sourceArray = source as Array;
            Array destinationArray;
            if (sourceArray == null)
            {
                destinationArray = ObjectCreator.CreateArray(destElementType, sourceLength);
            }
            else
            {
                destinationArray = ObjectCreator.CreateArray(destElementType, sourceArray);
                filler = new MultidimensionalArrayFiller(destinationArray);
            }
            foreach(var item in sourceList)
            {
                filler.NewValue(context.Map(item, null, typeof(TSourceElement), destElementType));
            }
            return destinationArray;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(MultidimensionalArrayMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsArray && context.DestinationType.GetArrayRank() > 1 && context.SourceType.IsEnumerableType();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(destExpression.Type, sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type)), sourceExpression, contextExpression);
        }
    }

    public class MultidimensionalArrayFiller
    {
        int[] indices;
        Array destination;

        public MultidimensionalArrayFiller(Array destination)
        {
            indices = new int[destination.Rank];
            this.destination = destination;
        }

        public void NewValue(object value)
        {
            int dimension = destination.Rank - 1;
            bool changedDimension = false;
            while(indices[dimension] == destination.GetLength(dimension))
            {
                indices[dimension] = 0;
                dimension--;
                if(dimension < 0)
                {
                    throw new InvalidOperationException("Not enough room in destination array " + destination);
                }
                indices[dimension]++;
                changedDimension = true;
            }
            destination.SetValue(value, indices);
            if(changedDimension)
            {
                indices[dimension+1]++;
            }
            else
            {
                indices[dimension]++;
            }
        }
    }
}