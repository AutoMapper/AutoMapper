using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;
    using System.Reflection;

    public class FieldAccessor<TSource, TValue> : FieldGetter<TSource, TValue>, IMemberAccessor
    {
        private readonly Lazy<Expression<LateBoundFieldSet<TSource, TValue>>> _lateBoundFieldSetExpression;
        private readonly Lazy<LateBoundFieldSet<TSource, TValue>> _lateBoundFieldSet;

        public FieldAccessor(FieldInfo fieldInfo)
            : base(fieldInfo)
        {
            _lateBoundFieldSetExpression = new Lazy<Expression<LateBoundFieldSet<TSource, TValue>>>(() => DelegateFactory.CreateSet<TSource, TValue>(fieldInfo));
            _lateBoundFieldSet = new Lazy<LateBoundFieldSet<TSource, TValue>>(() => _lateBoundFieldSetExpression.Value.Compile());
        }

        public void SetValue(object destination, object value)
        {
            _lateBoundFieldSet.Value((TSource)destination, value != null ? (TValue)value : default(TValue));
        }
    }
}