namespace AutoMapper.Execution
{
    using System.Reflection;

    public class ValueTypePropertyAccessor<TSource, TValue> : PropertyGetter<TSource, TValue>, IMemberAccessor
    {
        private readonly MethodInfo _lateBoundPropertySet;

        public ValueTypePropertyAccessor(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
            var method = propertyInfo.GetSetMethod(true);
            HasSetter = method != null;
            if (HasSetter)
            {
                _lateBoundPropertySet = method;
            }
        }

        public bool HasSetter { get; }

        public void SetValue(object destination, object value)
        {
            _lateBoundPropertySet.Invoke(destination, new[] {value});
        }
    }
}