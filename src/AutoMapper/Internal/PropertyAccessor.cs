namespace AutoMapper.Internal
{
    using System.Reflection;

    public class PropertyAccessor : PropertyGetter, IMemberAccessor
    {
        private readonly ILazy<LateBoundPropertySet> _lateBoundPropertySet;

        public PropertyAccessor(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
            HasSetter = propertyInfo.GetSetMethod(true) != null;
            if (HasSetter)
            {
                _lateBoundPropertySet = LazyFactory.Create(() => DelegateFactory.CreateSet(propertyInfo));
            }
        }

        public bool HasSetter { get; }

        public virtual void SetValue(object destination, object value)
        {
            _lateBoundPropertySet.Value(destination, value);
        }
    }
}