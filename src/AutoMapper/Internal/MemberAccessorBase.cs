using System;

namespace AutoMapper.Internal
{
	internal abstract class MemberAccessorBase : IMemberAccessor
	{
		public abstract string Name { get; }
		public abstract Type MemberType { get; }
		public abstract object GetValue(object source);
		public abstract void SetValue(object destination, object value);

		public ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
			       	? new ResolutionResult(source.Value, MemberType)
			       	: new ResolutionResult(GetValue(source.Value), MemberType);
		}
	}
}