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
            var destinationListType = GetDestinationListTypeFor(propertyMap);

            var sourceListType = result.Type.IsArray ? result.Type.GetElementType() : result.Type.GetTypeInfo().GenericTypeArguments.First();
            var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.MembersToExpand);

            Expression exp = result.ResolutionExpression;

            if (sourceListType != destinationListType)
            {
                var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(listTypePair, typePairCount);
                if(transformedExpression == null)
                {
                    return null;
                }
                exp = Expression.Call(
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
                exp = GetToListCallExpression(propertyMap, destinationListType, exp);                
            }
            else if (propertyMap.DestinationPropertyType.IsArray)
            {
                // Call .ToArray() on IEnumerable
                exp = Expression.Call(
                        typeof (Enumerable),
                        "ToArray",
                        new[] {destinationListType},
                        exp);
            }
            
            if(configuration.AllowNullCollections) {
                exp = Expression.Condition(
                            Expression.NotEqual(
                                Expression.TypeAs(result.ResolutionExpression, typeof(object)), 
                                Expression.Constant(null)),
                            exp,
                            Expression.Constant(null, propertyMap.DestinationPropertyType));
            }

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, exp);
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