using System;

namespace AutoMapper
{
	public abstract class TypeMember
	{
		public abstract object GetValue(object obj);
		public abstract Type GetMemberType();
	}
}