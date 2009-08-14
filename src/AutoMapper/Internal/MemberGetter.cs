using System;
using System.Reflection;

namespace AutoMapper.Internal
{
	internal abstract class MemberGetter : IMemberGetter
	{
		public abstract string Name { get; }
		public abstract Type MemberType { get; }
		public abstract object GetValue(object source);

		public ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
			       	? new ResolutionResult(source.Value, MemberType)
			       	: new ResolutionResult(GetValue(source.Value), MemberType);
		}

	}
}