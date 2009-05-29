using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class MethodAccessor : MemberAccessorBase
	{
		private readonly MethodInfo _methodInfo;
		private readonly LateBoundMethod _lateBoundMethod;

		public MethodAccessor(MethodInfo methodInfo)
		{
			_methodInfo = methodInfo;
			_lateBoundMethod = DelegateFactory.Create(methodInfo);
		}

		public override string Name
		{
			get { return _methodInfo.Name; }
		}

		public override Type MemberType
		{
			get { return _methodInfo.ReturnType; }
		}

		public override object GetValue(object source)
		{
			return MemberType == null
			       	? null
					: _lateBoundMethod(source, new object[0]);
		}

		public override void SetValue(object destination, object value)
		{
			if (MemberType == null)
			{
				_methodInfo.Invoke(destination, new[] {value});
			}
		}

	    public bool Equals(MethodAccessor other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Equals(other._methodInfo, _methodInfo);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != typeof (MethodAccessor)) return false;
	        return Equals((MethodAccessor) obj);
	    }

	    public override int GetHashCode()
	    {
	        return _methodInfo.GetHashCode();
	    }
	}
}