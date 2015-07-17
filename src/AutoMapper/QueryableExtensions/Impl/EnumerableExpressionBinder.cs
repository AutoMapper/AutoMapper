namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internal;

    public class EnumerableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                   propertyMap.DestinationPropertyType != typeof(string);
        }

        public MemberAssignment Build(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap,
            ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindEnumerableExpression(mappingEngine, propertyMap, request, result, typePairCount);
        }

        private static MemberAssignment BindEnumerableExpression(IMappingEngine mappingEngine, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            MemberAssignment bindExpression;
            Type destinationListType = GetDestinationListTypeFor(propertyMap);
            Type sourceListType = null;
            // is list

            if (result.Type.IsArray)
            {
                sourceListType = result.Type.GetElementType();
            }
            else
            {
                sourceListType = result.Type.GetGenericArguments().First();
            }
            var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.IncludedMembers);


            var selectExpression = result.ResolutionExpression;
            if (sourceListType != destinationListType)
            {
                var transformedExpression = AutoMapper.QueryableExtensions.Extensions.CreateMapExpression(mappingEngine, listTypePair, typePairCount);
                selectExpression = Expression.Call(
                    typeof(Enumerable),
                    "Select",
                    new[] { sourceListType, destinationListType },
                    result.ResolutionExpression,
                    transformedExpression);
            }

            if (typeof(IList<>).MakeGenericType(destinationListType).IsAssignableFrom(propertyMap.DestinationPropertyType)
                || typeof(ICollection<>).MakeGenericType(destinationListType).IsAssignableFrom(propertyMap.DestinationPropertyType))
            {
                // Call .ToList() on IEnumerable
                var toListCallExpression = GetToListCallExpression(propertyMap, destinationListType, selectExpression);

                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, toListCallExpression);
            }
            else if (propertyMap.DestinationPropertyType.IsArray)
            {
                // Call .ToArray() on IEnumerable
                MethodCallExpression toArrayCallExpression = Expression.Call(
                    typeof(Enumerable),
                    "ToArray",
                    new Type[] { destinationListType },
                    selectExpression);
                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, toArrayCallExpression);
            }
            else
            {
                // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, selectExpression);
            }
            return bindExpression;
        }

        private static Type GetDestinationListTypeFor(PropertyMap propertyMap)
        {
            Type destinationListType;
            if (propertyMap.DestinationPropertyType.IsArray)
                destinationListType = propertyMap.DestinationPropertyType.GetElementType();
            else
                destinationListType = propertyMap.DestinationPropertyType.GetGenericArguments().First();
            return destinationListType;
        }

        private static MethodCallExpression GetToListCallExpression(PropertyMap propertyMap, Type destinationListType,
            Expression selectExpression)
        {
            return Expression.Call(
                typeof(Enumerable),
                propertyMap.DestinationPropertyType.IsArray ? "ToArray" : "ToList",
                new[] { destinationListType },
                selectExpression);
        }
    }
}