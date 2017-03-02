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

        private static Array Map<TDestination, TSource, TSourceElement>(TSource source, ResolutionContext context, bool allowNullCollections, bool allowNullDestinationValues)
            where TSource : IEnumerable
        {
            if (source == null && allowNullCollections)
                return null;

            var destElementType = TypeHelper.GetElementType(typeof(TDestination));

            if (source != null && typeof(TDestination).IsAssignableFrom(typeof(TSource)))
            {
                var elementTypeMap = context.ConfigurationProvider.ResolveTypeMap(typeof(TSourceElement), destElementType);
                if (elementTypeMap == null)
                    return source as Array;
            }

            IEnumerable sourceList = (IEnumerable) source ??
                                     (typeof(TSource).GetTypeInfo().IsInterface
                                         ? new List<TSourceElement>()
                                         : (IEnumerable<TSourceElement>) (
                                             allowNullDestinationValues
                                                 ? ObjectCreator.CreateNonNullValue(typeof(TSource))
                                                 : ObjectCreator.CreateObject(typeof(TSource)))
                                     );

            var sourceLength = sourceList.OfType<object>().Count();
            var sourceArray = source as Array;
            Array destinationArray;
            if (sourceArray == null)
            {
                destinationArray = Array.CreateInstance(destElementType, sourceLength);
            }
            else
            {
                destinationArray = Array.CreateInstance(destElementType, sourceArray.GetLengths());
                filler = new MultidimensionalArrayFiller(destinationArray);
            }
            foreach(var item in sourceList)
            {
                filler.NewValue(context.Map(item, null, typeof(TSourceElement), destElementType));
            }
            return destinationArray;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(MultidimensionalArrayMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsArray && context.DestinationType.GetArrayRank() > 1 && context.SourceType.IsEnumerableType();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, 
                MapMethodInfo.MakeGenericMethod(destExpression.Type, sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type)), 
                sourceExpression, 
                contextExpression,
                Expression.Constant(propertyMap?.TypeMap.Profile.AllowNullCollections ??
                                       configurationProvider.Configuration.AllowNullCollections),
                Expression.Constant(propertyMap?.TypeMap.Profile.AllowNullDestinationValues ??
                                       configurationProvider.Configuration.AllowNullDestinationValues));
        }

        public class MultidimensionalArrayFiller
        {
            private readonly int[] _indices;
            private readonly Array _destination;

            public MultidimensionalArrayFiller(Array destination)
            {
                _indices = new int[destination.Rank];
                _destination = destination;
            }

            public void NewValue(object value)
            {
                int dimension = _destination.Rank - 1;
                bool changedDimension = false;
                while (_indices[dimension] == _destination.GetLength(dimension))
                {
                    _indices[dimension] = 0;
                    dimension--;
                    if (dimension < 0)
                    {
                        throw new InvalidOperationException("Not enough room in destination array " + _destination);
                    }
                    _indices[dimension]++;
                    changedDimension = true;
                }
                _destination.SetValue(value, _indices);
                if (changedDimension)
                {
                    _indices[dimension + 1]++;
                }
                else
                {
                    _indices[dimension]++;
                }
            }
        }
    }

}