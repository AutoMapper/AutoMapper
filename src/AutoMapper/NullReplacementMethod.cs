using System;

namespace AutoMapper
{
	public class NullReplacementMethod : IValueResolver
	{
		private readonly IValueResolver _member;
		private readonly string _nullSubstitute;

		public NullReplacementMethod(IValueResolver member, string nullSubstitute)
		{
			_member = member;
			_nullSubstitute = nullSubstitute;
		}

		public object Resolve(object obj)
		{
			obj = _member.Resolve(obj);
			if (obj == null)
				return _nullSubstitute;
			return obj;
		}

		public Type GetResolvedValueType()
		{
			return _member.GetResolvedValueType();
		}
	}
}
