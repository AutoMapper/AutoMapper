using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal class MethodAccessor : MemberAccessorBase
	{
		private readonly MethodInfo _methodInfo;

		public MethodAccessor(MethodInfo methodInfo)
		{
			_methodInfo = methodInfo;
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
			       	: _methodInfo.Invoke(source, new object[0]);
		}

		public override void SetValue(object destination, object value)
		{
			if (MemberType == null)
			{
				_methodInfo.Invoke(destination, new[] {value});
			}
		}
	}
}