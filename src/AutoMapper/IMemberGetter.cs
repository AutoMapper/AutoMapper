using System;
using System.Reflection;

namespace AutoMapper
{
	public interface IMemberGetter : IValueResolver
	{
		MemberInfo MemberInfo { get; }
		string Name { get; }
		Type MemberType { get; }
		object GetValue(object source);
	}

	public interface IMemberAccessor : IMemberGetter
	{
		void SetValue(object destination, object value);
	}
}