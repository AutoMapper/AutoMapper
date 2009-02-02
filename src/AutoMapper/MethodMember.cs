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

		public override Type GetResolvedValueType()
		{
			return _method.ReturnType;
		}

		public override ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
			       	? new ResolutionResult(source.Value, _method.ReturnType)
			       	: new ResolutionResult(_method.Invoke(source.Value, new object[0]), _method.ReturnType);
		}
	}
}