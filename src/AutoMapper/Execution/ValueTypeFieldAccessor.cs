namespace AutoMapper.Execution
{
    using System.Reflection;

    public class ValueTypeFieldAccessor<TSource, TValue> : FieldGetter<TSource, TValue>, IMemberAccessor
    {
        private readonly FieldInfo _lateBoundFieldSet;

        public ValueTypeFieldAccessor(FieldInfo fieldInfo)
            : base(fieldInfo)
        {
            _lateBoundFieldSet = fieldInfo;
        }

        public void SetValue(object destination, object value)
        {
            _lateBoundFieldSet.SetValue(destination, value);
        }
    }
}