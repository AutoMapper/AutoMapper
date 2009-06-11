using System;

namespace AutoMapper
{
	internal class DeferredInstantiatedResolver : IValueResolver
	{
		private readonly Func<IValueResolver> _constructor;

		public DeferredInstantiatedResolver(Func<IValueResolver> constructor)
		{
			_constructor = constructor;
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
		    var resolver = _constructor();

		    return resolver.Resolve(source);
		}
	}
}