using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class MethodGetter : MemberGetter
	{
		private readonly MethodInfo _methodInfo;
		private readonly string _name;
		private readonly Type _memberType;
		private readonly LateBoundMethod _lateBoundMethod;

		public MethodGetter(MethodInfo methodInfo)
		{
			_methodInfo = methodInfo;
			_name = _methodInfo.Name;
			_memberType = _methodInfo.ReturnType;
			_lateBoundMethod = DelegateFactory.CreateGet(methodInfo);
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
					: _lateBoundMethod(source, new object[0]);
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