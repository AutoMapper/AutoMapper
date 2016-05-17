using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class FieldGetter<TSource, TValue> : MemberGetter<TSource, TValue>
    {
        private readonly FieldInfo _fieldInfo;
        private readonly Lazy<Expression<LateBoundFieldGet<TSource, TValue>>> _lateBoundFieldGetExpression;
        private readonly Lazy<LateBoundFieldGet<TSource, TValue>> _lateBoundFieldGet;

        public FieldGetter(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
            Name = fieldInfo.Name;
            MemberType = fieldInfo.FieldType;
            _lateBoundFieldGetExpression = new Lazy<Expression<LateBoundFieldGet<TSource, TValue>>>(() => DelegateFactory.CreateGet<TSource, TValue>(fieldInfo));
            _lateBoundFieldGet = new Lazy<LateBoundFieldGet<TSource, TValue>>(() => _lateBoundFieldGetExpression.Value.Compile());
        }

        public override MemberInfo MemberInfo => _fieldInfo;

        public override string Name { get; }
        public override LambdaExpression GetExpression => _lateBoundFieldGetExpression.Value;

        public override Type MemberType { get; }

        public override object GetValue(object source)
        {
            return _lateBoundFieldGet.Value((TSource)source);
        }

        public bool Equals(FieldGetter<TSource, TValue> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._fieldInfo, _fieldInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (FieldGetter<TSource, TValue>)) return false;
            return Equals((FieldGetter<TSource, TValue>) obj);
        }

        public override int GetHashCode()
        {
            return _fieldInfo.GetHashCode();
        }

        public override IEnumerable<object> GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _fieldInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override IEnumerable<object> GetCustomAttributes(bool inherit)
        {
            return _fieldInfo.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _fieldInfo.IsDefined(attributeType, inherit);
        }
    }
}