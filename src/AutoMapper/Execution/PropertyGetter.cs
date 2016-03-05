using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class PropertyGetter<TSource, TValue> : MemberGetter<TSource, TValue>
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly Lazy<Expression<LateBoundPropertyGet<TSource, TValue>>> _lateBoundPropertyGetExpression;
        private readonly Lazy<LateBoundPropertyGet<TSource, TValue>> _lateBoundPropertyGet;

        public PropertyGetter(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            Name = _propertyInfo.Name;
            MemberType = _propertyInfo.PropertyType;
            _lateBoundPropertyGetExpression =
                _propertyInfo.GetGetMethod(true) != null 
                ? new Lazy<Expression<LateBoundPropertyGet<TSource, TValue>>>(() => DelegateFactory.CreateGet<TSource, TValue>(propertyInfo)) 
                : new Lazy<Expression<LateBoundPropertyGet<TSource, TValue>>>(() => src => default(TValue));
            _lateBoundPropertyGet =
                new Lazy<LateBoundPropertyGet<TSource, TValue>>(() => _lateBoundPropertyGetExpression.Value.Compile());
        }

        public override MemberInfo MemberInfo => _propertyInfo;

        public override string Name { get; }
        public override LambdaExpression GetExpression => _lateBoundPropertyGetExpression.Value;

        public override Type MemberType { get; }

        public override object GetValue(object source)
        {
            return _lateBoundPropertyGet.Value((TSource)source);
        }

        public override IEnumerable<object> GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _propertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override IEnumerable<object> GetCustomAttributes(bool inherit)
        {
            return _propertyInfo.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _propertyInfo.IsDefined(attributeType, inherit);
        }

        public bool Equals(PropertyGetter<TSource, TValue> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._propertyInfo, _propertyInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PropertyGetter<TSource, TValue>)) return false;
            return Equals((PropertyGetter<TSource, TValue>) obj);
        }

        public override int GetHashCode()
        {
            return _propertyInfo.GetHashCode();
        }
    }
}