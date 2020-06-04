using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Mappers.Internal.ElementTypeHelper;
    using static Expression;
    public abstract class EnumBinder : IExpressionBinder
    {
        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Bind(propertyMap.DestinationMember, Convert(result.ResolutionExpression, propertyMap.DestinationType));
        public abstract bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result);
    }
    public class EnumToUnderlyingTypeBinder : EnumBinder
    {
        public override bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => IsEnumToUnderlyingType(propertyMap.Types);
    }
    public class UnderlyingTypeToEnumBinder : EnumBinder
    {
        public override bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => IsUnderlyingTypeToEnum(propertyMap.Types);
    }
    public class EnumToEnumBinder : EnumBinder
    {
        public override bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => IsEnumToEnum(propertyMap.Types);
    }
}