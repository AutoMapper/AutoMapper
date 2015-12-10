namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class FieldGetter : MemberGetter
    {
        private readonly FieldInfo _fieldInfo;
        private readonly Lazy<LateBoundFieldGet> _lateBoundFieldGet;

        public FieldGetter(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
            Name = fieldInfo.Name;
            MemberType = fieldInfo.FieldType;
            _lateBoundFieldGet = new Lazy<LateBoundFieldGet>(() => DelegateFactory.CreateGet(fieldInfo));
        }

        public override MemberInfo MemberInfo => _fieldInfo;

        public override string Name { get; }

        public override Type MemberType { get; }

        public override object GetValue(object source)
        {
            return _lateBoundFieldGet.Value(source);
        }

        public bool Equals(FieldGetter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._fieldInfo, _fieldInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (FieldGetter)) return false;
            return Equals((FieldGetter) obj);
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