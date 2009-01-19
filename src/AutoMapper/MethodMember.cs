using System;
using System.Reflection;

namespace AutoMapper
{
	internal class MethodMember : TypeMember
	{
		private readonly MethodInfo _method;

		public MethodMember(MethodInfo method)
		{
			_method = method;
		}

		public override object GetValue(object obj)
		{
			return _method.Invoke(obj, new object[0]);
		}

		public override Type GetMemberType()
		{
			return _method.ReturnType;
		}
	}
}