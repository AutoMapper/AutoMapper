using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;
    using System.Reflection;

    public class PropertyAccessor<TSource, TValue> : PropertyGetter<TSource, TValue>, IMemberAccessor
    {
        private readonly Lazy<Expression<LateBoundPropertySet<TSource, TValue>>> _lateBoundPropertySetExpression;
        private readonly Lazy<LateBoundPropertySet<TSource, TValue>> _lateBoundPropertySet;

        public PropertyAccessor(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
            var HasSetter = propertyInfo.GetSetMethod(true) != null;
            _lateBoundPropertySetExpression = HasSetter
                ? new Lazy<Expression<LateBoundPropertySet<TSource, TValue>>>(() => DelegateFactory.CreateSet<TSource, TValue>(propertyInfo))
                : new Lazy<Expression<LateBoundPropertySet<TSource, TValue>>>(() => Expression.Lambda<LateBoundPropertySet<TSource, TValue>>(Expression.Empty(), Expression.Parameter(typeof(TSource)), Expression.Parameter(typeof(TValue))));
            _lateBoundPropertySet = new Lazy<LateBoundPropertySet<TSource, TValue>>(() => _lateBoundPropertySetExpression.Value.Compile());
        }

        public virtual void SetValue(object destination, object value)
        {
            _lateBoundPropertySet.Value((TSource)destination, (TValue)value);
        }
    }
}