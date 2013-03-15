using System;
using System.Reflection;

namespace AutoMapper
{
	public interface IMemberResolver : IValueResolver
	{
		Type MemberType { get; }
	}

	public interface IMemberGetter : IMemberResolver
	{
		MemberInfo MemberInfo { get; }
		string Name { get; }
		object GetValue(object source);
	}

	public interface IMemberAccessor : IMemberGetter
	{
		void SetValue(object destination, object value);
	}
}