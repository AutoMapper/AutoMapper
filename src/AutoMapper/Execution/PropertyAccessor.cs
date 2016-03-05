namespace AutoMapper.Execution
{
    using System;
    using System.Reflection;

    public class PropertyAccessor : PropertyGetter, IMemberAccessor
    {
        private readonly Lazy<LateBoundPropertySet> _lateBoundPropertySet;

        public PropertyAccessor(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
            var HasSetter = propertyInfo.GetSetMethod(true) != null;
            _lateBoundPropertySet = HasSetter
                ? new Lazy<LateBoundPropertySet>(() => DelegateFactory.CreateSet(propertyInfo))
                : new Lazy<LateBoundPropertySet>(() => (_, __) => { });
        }

        public virtual void SetValue(object destination, object value)
        {
            _lateBoundPropertySet.Value(destination, value);
        }
    }
}