using System;

namespace AutoMapper.Internal
{
    using System.Reflection;

    public class FieldAccessor : FieldGetter, IMemberAccessor
    {
        private readonly Lazy<LateBoundFieldSet> _lateBoundFieldSet;

        public FieldAccessor(FieldInfo fieldInfo)
            : base(fieldInfo)
        {
            _lateBoundFieldSet = new Lazy<LateBoundFieldSet>(() => DelegateFactory.CreateSet(fieldInfo));
        }

        public void SetValue(object destination, object value)
        {
            _lateBoundFieldSet.Value(destination, value);
        }
    }
}