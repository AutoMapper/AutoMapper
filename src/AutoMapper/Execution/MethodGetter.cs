using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class MethodGetter<TSource, TValue> : MemberGetter<TSource, TValue>
    {
        private readonly MethodInfo _methodInfo;
        private readonly Type _memberType;
        private readonly Lazy<Expression<LateBoundMethod<object, TValue>>> _lateBoundMethodExpression;
        private readonly Lazy<LateBoundMethod<object, TValue>> _lateBoundMethod;

        public MethodGetter(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            Name = _methodInfo.Name;
            _memberType = _methodInfo.ReturnType;
            _lateBoundMethodExpression = new Lazy<Expression<LateBoundMethod<object, TValue>>>(() => DelegateFactory.CreateGet<TValue>(methodInfo));
            _lateBoundMethod = new Lazy<LateBoundMethod<object, TValue>>(() => _lateBoundMethodExpression.Value.Compile());
        }

        public override MemberInfo MemberInfo => _methodInfo;

        public override string Name { get; }
        public override LambdaExpression GetExpression => _lateBoundMethodExpression.Value;

        public override Type MemberType => _memberType;

        public override object GetValue(object source)
        {
            return _memberType == null
                ? default(TValue)
                : _lateBoundMethod.Value(source, new object[0]);
        }

        public override IEnumerable<object> GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _methodInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override IEnumerable<object> GetCustomAttributes(bool inherit)
        {
            return _methodInfo.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _methodInfo.IsDefined(attributeType, inherit);
        }

        public bool Equals(MethodGetter<TSource, TValue> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._methodInfo, _methodInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MethodGetter<TSource, TValue>)) return false;
            return Equals((MethodGetter<TSource, TValue>) obj);
        }

        public override int GetHashCode()
        {
            return _methodInfo.GetHashCode();
        }
    }
}