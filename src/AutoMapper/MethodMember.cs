using System;
using System.Reflection;

namespace AutoMapper
{
	internal class MethodMember : IValueResolver
	{
		private readonly MethodInfo _method;

		public MethodMember(MethodInfo method)
		{
			_method = method;
		}

		public object Resolve(object obj)
		{
			return _method.Invoke(obj, new object[0]);
		}

		public Type GetResolvedValueType()
		{
			return _method.ReturnType;
		}
	}
}