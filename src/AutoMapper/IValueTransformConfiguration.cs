using System.Linq.Expressions;

namespace AutoMapper
{
    public interface IValueTransformConfiguration
    {
        bool IsMatch(PropertyMap propertyMap);
        Expression Visit(Expression current, PropertyMap propertyMap);
    }
}