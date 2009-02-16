using System;

namespace AutoMapper
{
	public interface IMemberAccessor : IValueResolver
	{
		string Name { get; }
		Type MemberType { get; }
		object GetValue(object source);
		void SetValue(object destination, object value);
	}
}