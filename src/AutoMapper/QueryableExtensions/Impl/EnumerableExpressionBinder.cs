namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
////TODO: TBD: not sure this would be necessary after all...
//#if MONODROID
//    using Extensions = AutoMapper.QueryableExtensions.Extensions;
//#endif

    /// <summary>
    /// 
    /// </summary>
    public class EnumerableExpressionBinder : IExpressionBinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyMap"></param>
        /// <param name="propertyTypeMap"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                   propertyMap.DestinationPropertyType != typeof (string);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappingEngine"></param>
        /// <param name="propertyMap"></param>
        /// <param name="propertyTypeMap"></param>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="typePairCount"></param>
        /// <returns></returns>
        public MemberAssignment Build(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap,
            ExpressionRequest request, ExpressionResolutionResult result,
            Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            MemberAssignment bindExpression;

            var destinationListType = GetDestinationListTypeFor(propertyMap);
            var sourceListType = result.Type.IsArray
                ? result.Type.GetElementType()
                : result.Type.GetGenericArguments().First();

            var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.IncludedMembers);
            var selectExpression = result.ResolutionExpression;

            if (sourceListType != destinationListType)
            {
                const string methodName = nameof(Enumerable.Select);
                var transformedExpression = mappingEngine.CreateMapExpression(listTypePair, typePairCount);
                selectExpression = Expression.Call(
                    typeof (Enumerable),
                    methodName,
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
                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, toListCallExpression);
            }
            else if (propertyMap.DestinationPropertyType.IsArray)
            {
                // Call .ToArray() on IEnumerable
                const string methodName = nameof(Enumerable.ToArray);
                var toArrayCallExpression = Expression.Call(
                    typeof (Enumerable),
                    methodName,
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
                : propertyMap.DestinationPropertyType.GetGenericArguments().First();
            return destinationListType;
        }

        private static MethodCallExpression GetToListCallExpression(PropertyMap propertyMap, Type destinationListType,
            Expression selectExpression)
        {
            var methodName = propertyMap.DestinationPropertyType.IsArray
                ? nameof(Enumerable.ToArray)
                : nameof(Enumerable.ToList);
            return Expression.Call(typeof (Enumerable), methodName,
                new[] {destinationListType}, selectExpression);
        }
    }
}