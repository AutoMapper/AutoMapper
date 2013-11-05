namespace AutoMapper
{
    using System;

	public class DelegateBasedResolver<TSource> : IValueResolver
	{
		private readonly Func<TSource, object> _method;

		public DelegateBasedResolver(Func<TSource, object> method)
		{
			_method = method;
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value != null && ! (source.Value is TSource))
			{
                throw new ArgumentException("Expected obj to be of type " + typeof (TSource) + " but was " +
                                            source.Value.GetType());
			}

            var result = _method((TSource) source.Value);

			return source.New(result);
		}
	}
}
