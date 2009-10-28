using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal abstract class MemberGetter : IMemberGetter
	{
		public abstract MemberInfo MemberInfo { get; }
		public abstract string Name { get; }
		public abstract Type MemberType { get; }
		public abstract object GetValue(object source);

		public ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
			       	? source.New(source.Value, MemberType)
			       	: source.New(GetValue(source.Value), MemberType);
		}

	}
}