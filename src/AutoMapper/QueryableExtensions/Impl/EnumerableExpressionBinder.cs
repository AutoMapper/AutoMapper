namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
#if MONODROID
    using Extensions = AutoMapper.QueryableExtensions.Extensions;
#endif

    public class EnumerablePropertyProjector : IPropertyProjector
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                   propertyMap.DestinationPropertyType != typeof (string);
        }

        public Expression Project(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap,
            ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BuildEnumerableExpression(mappingEngine, propertyMap, request, result, typePairCount);
        }

        private static Expression BuildEnumerableExpression(IMappingEngine mappingEngine, PropertyMap propertyMap,
            ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            Type destinationListType = GetDestinationListTypeFor(propertyMap);

            var sourceListType = result.Type.IsArray ? result.Type.GetElementType() : result.Type.GetGenericArguments().First();
            var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.MembersToExpand);

            var selectExpression = result.ResolutionExpression;
            if (sourceListType != destinationListType)
            {
                var transformedExpression = ((IMappingEngineRunner)mappingEngine).CreateMapExpression(listTypePair, typePairCount);
                selectExpression = Expression.Call(
                    typeof (Enumerable),
                    "Select",
                    new[] {sourceListType, destinationListType},
                    result.ResolutionExpression,
                    transformedExpression);
            }

            if (typeof (IList<>).MakeGenericType(destinationListType)
                .IsAssignableFrom(propertyMap.DestinationPropertyType)
                ||
                typeof (ICollection<>).MakeGenericType(destinationListType)
                    .IsAssignableFrom(propertyMap.DestinationPropertyType))
            {
                // Call .ToList() on IEnumerable
                var toListCallExpression = GetToListCallExpression(propertyMap, destinationListType, selectExpression);

                return toListCallExpression;
            }
            else if (propertyMap.DestinationPropertyType.IsArray)
            {
                // Call .ToArray() on IEnumerable
                MethodCallExpression toArrayCallExpression = Expression.Call(
                    typeof (Enumerable),
                    "ToArray",
                    new[] {destinationListType},
                    selectExpression);

                return toArrayCallExpression;
            }
            else
            {
                // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                return selectExpression;
            }
        }

        private static Type GetDestinationListTypeFor(PropertyMap propertyMap)
        {
            var destinationListType = propertyMap.DestinationPropertyType.IsArray 
                ? propertyMap.DestinationPropertyType.GetElementType() 
                : propertyMap.DestinationPropertyType.GetGenericArguments().First();
            return destinationListType;
        }

        private static MethodCallExpression GetToListCallExpression(PropertyMap propertyMap, Type destinationListType,
            Expression sourceExpression)
        {
            return Expression.Call(
                typeof (Enumerable),
                propertyMap.DestinationPropertyType.IsArray ? "ToArray" : "ToList",
                new[] {destinationListType},
                sourceExpression);
        }
    }
}