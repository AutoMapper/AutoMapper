using System;

namespace AutoMapper
{
	internal class DelegateBasedResolver<TSource, TMember> : IMemberResolver
	{
		private readonly Func<TSource, TMember> _method;

		public DelegateBasedResolver(Func<TSource, TMember> method)
		{
			_method = method;
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value != null && ! (source.Value is TSource))
			{
				throw new ArgumentException("Expected obj to be of type " + typeof(TSource) + " but was " + source.Value.GetType());
			}

		    try
			{
			    var result = _method((TSource)source.Value);
			    
                return source.New(result, MemberType);
			}
			catch (NullReferenceException)
			{
			    return source.New(null, MemberType);
			}
		}

		public Type MemberType
		{
			get { return typeof (TMember); }
		}
	}
}