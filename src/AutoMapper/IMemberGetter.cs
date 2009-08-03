using System;

namespace AutoMapper
{
	public interface IMemberGetter : IValueResolver
	{
		string Name { get; }
		Type MemberType { get; }
		object GetValue(object source);
	}

	public interface IMemberAccessor : IMemberGetter
	{
		void SetValue(object destination, object value);
	}

}