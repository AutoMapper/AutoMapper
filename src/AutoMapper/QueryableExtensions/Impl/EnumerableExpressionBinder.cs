using System.Collections.Concurrent;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Linq.Expressions;

    public class EnumerableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.GetTypeInfo().ImplementedInterfaces.Any(t => t.Name == "IEnumerable") &&
                   propertyMap.DestinationPropertyType != typeof (string);
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindEnumerableExpression(configuration, propertyMap, request, result, typePairCount);
        }

        private static MemberAssignment BindEnumerableExpression(IConfigurationProvider configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            MemberAssignment bindExpression;
            Type destinationListType = GetDestinationListTypeFor(propertyMap);

            var sourceListType = result.Type.IsArray ? result.Type.GetElementType() : result.Type.GetTypeInfo().GenericTypeArguments.First();
            var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.MembersToExpand);

            var selectExpression = result.ResolutionExpression;
            if (sourceListType != destinationListType)
            {
                var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(listTypePair, typePairCount);
                if(transformedExpression == null)
                {
                    return null;
                }
                selectExpression = Expression.Call(
                    typeof (Enumerable),
                    "Select",
                    new[] {sourceListType, destinationListType},
                    result.ResolutionExpression,
                    transformedExpression);
            }

            if (typeof (IList<>).MakeGenericType(destinationListType)
                .GetTypeInfo().IsAssignableFrom(propertyMap.DestinationPropertyType.GetTypeInfo())
                ||
                typeof (ICollection<>).MakeGenericType(destinationListType)
                    .GetTypeInfo().IsAssignableFrom(propertyMap.DestinationPropertyType.GetTypeInfo()))
            {
                // Call .ToList() on IEnumerable
                var toListCallExpression = GetToListCallExpression(propertyMap, destinationListType, selectExpression);

                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, toListCallExpression);
            }
            else if (propertyMap.DestinationPropertyType.IsArray)
            {
                // Call .ToArray() on IEnumerable
                MethodCallExpression toArrayCallExpression = Expression.Call(
                    typeof (Enumerable),
                    "ToArray",
                    new[] {destinationListType},
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
            var destinationListType = propertyMap.DestinationPropertyType.IsArray 
                ? propertyMap.DestinationPropertyType.GetElementType() 
                : propertyMap.DestinationPropertyType.GetTypeInfo().GenericTypeArguments.First();
            return destinationListType;
        }

        private static MethodCallExpression GetToListCallExpression(PropertyMap propertyMap, Type destinationListType,
            Expression selectExpression)
        {
            return Expression.Call(
                typeof (Enumerable),
                propertyMap.DestinationPropertyType.IsArray ? "ToArray" : "ToList",
                new[] {destinationListType},
                selectExpression);
        }
    }
}