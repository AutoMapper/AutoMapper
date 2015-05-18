using System;
using System.Reflection;

namespace AutoMapper.Impl
{
    using System.Collections.Generic;
    using Internal;

    public class MethodGetter : MemberGetter
	{
		private readonly MethodInfo _methodInfo;
		private readonly string _name;
		private readonly Type _memberType;
		private readonly ILazy<LateBoundMethod> _lateBoundMethod;

		public MethodGetter(MethodInfo methodInfo)
		{
			_methodInfo = methodInfo;
			_name = _methodInfo.Name;
			_memberType = _methodInfo.ReturnType;
            _lateBoundMethod = LazyFactory.Create(() => DelegateFactory.CreateGet(methodInfo));
		}

		public override MemberInfo MemberInfo
		{
			get { return _methodInfo; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override Type MemberType
		{
			get { return _memberType; }
		}

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
			if (obj.GetType() != typeof(MethodGetter)) return false;
			return Equals((MethodGetter)obj);
		}

		public override int GetHashCode()
		{
			return _methodInfo.GetHashCode();
		}
	}
}