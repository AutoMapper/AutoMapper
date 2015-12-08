namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class MethodGetter : MemberGetter
    {
        private readonly MethodInfo _methodInfo;
        private readonly Type _memberType;
        private readonly Lazy<LateBoundMethod> _lateBoundMethod;

        public MethodGetter(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            Name = _methodInfo.Name;
            _memberType = _methodInfo.ReturnType;
            _lateBoundMethod = new Lazy<LateBoundMethod>(() => DelegateFactory.CreateGet(methodInfo));
        }

        public override MemberInfo MemberInfo => _methodInfo;

        public override string Name { get; }

        public override Type MemberType => _memberType;

        public override object GetValue(object source)
        {
            return _memberType == null
                ? null
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

        public bool Equals(MethodGetter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._methodInfo, _methodInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MethodGetter)) return false;
            return Equals((MethodGetter) obj);
        }

        public override int GetHashCode()
        {
            return _methodInfo.GetHashCode();
        }
    }
}