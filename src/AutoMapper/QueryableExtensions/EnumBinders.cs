using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class EnumBinder : IExpressionBinder
    {
        public Expression Build(IGlobalConfiguration configuration, IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Convert(result.ResolutionExpression, propertyMap.DestinationType);
        public abstract bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumToUnderlyingTypeBinder : EnumBinder
    {
        public override bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => propertyMap.Types.IsEnumToUnderlyingType();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UnderlyingTypeToEnumBinder : EnumBinder
    {
        public override bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => propertyMap.Types.IsUnderlyingTypeToEnum();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumToEnumBinder : EnumBinder
    {
        public override bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => propertyMap.Types.IsEnumToEnum();
    }
}