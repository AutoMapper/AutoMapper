namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class PropertyGetter : MemberGetter
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly ILazy<LateBoundPropertyGet> _lateBoundPropertyGet;

        public PropertyGetter(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            Name = _propertyInfo.Name;
            MemberType = _propertyInfo.PropertyType;
            _lateBoundPropertyGet =
                _propertyInfo.GetGetMethod(true) != null 
                ? LazyFactory.Create(() => DelegateFactory.CreateGet(propertyInfo)) 
                : LazyFactory.Create<LateBoundPropertyGet>(() => src => null);
        }

        public override MemberInfo MemberInfo => _propertyInfo;

        public override string Name { get; }

        public override Type MemberType { get; }

        public override object GetValue(object source)
        {
            return _lateBoundPropertyGet.Value(source);
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

        public bool Equals(PropertyGetter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._propertyInfo, _propertyInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PropertyGetter)) return false;
            return Equals((PropertyGetter) obj);
        }

        public override int GetHashCode()
        {
            return _propertyInfo.GetHashCode();
        }
    }
}