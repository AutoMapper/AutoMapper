namespace AutoMapper.Execution
{
    using System.Reflection;

    public class ValueTypePropertyAccessor<TSource, TValue> : PropertyGetter<TSource, TValue>, IMemberAccessor
    {
        private readonly LateBoundPropertySet<TSource, TValue> _lateBoundPropertySetExpression;
        private readonly LateBoundPropertySet<TSource, TValue> _lateBoundPropertySet;

        public ValueTypePropertyAccessor(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
            var method = propertyInfo.GetSetMethod(true);
            HasSetter = method != null;
            if (HasSetter)
            {
                _lateBoundPropertySetExpression = ;
            }
            _lateBoundPropertySet = (_, __) => { };
        }

        public bool HasSetter { get; }

        public void SetValue(object destination, object value)
        {
            _lateBoundPropertySet.Invoke(destination, new[] {value});
        }
    }
}