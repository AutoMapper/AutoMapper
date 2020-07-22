using System;
using System.ComponentModel;
using System.Reflection;

namespace AutoMapper
{
    /// <summary>
    /// Used to ensure we capture the concrete parent type. For virtual members, AutoMapper's GetMemberInfo methods will return a
    /// MemberInfo whose ReflectedType is the base class - not source or destination being mapped.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MapMemberInfo : MemberInfo, IEquatable<MemberInfo>
    {
        public MapMemberInfo(MemberInfo memberInfo, Type parentType)
        {
            _memberInfo = memberInfo;
            _parentType = parentType;
        }

        private MemberInfo _memberInfo { get; set; }
        private Type _parentType { get; set; }

        public override Type DeclaringType => _memberInfo.DeclaringType;

        public override MemberTypes MemberType => _memberInfo.MemberType;

        public override string Name => _memberInfo.Name;

        public override Type ReflectedType => _parentType;

        public bool Equals(MemberInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return this._memberInfo.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            MemberInfo mInfo = obj as MemberInfo;
            if (mInfo == null) return false;

            return Equals(mInfo);
        }

        public override object[] GetCustomAttributes(bool inherit) => _memberInfo.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _memberInfo.GetCustomAttributes(attributeType, inherit);

        public override int GetHashCode() => _memberInfo.GetHashCode();

        public override bool IsDefined(Type attributeType, bool inherit) => _memberInfo.IsDefined(attributeType, inherit);
    }
}
