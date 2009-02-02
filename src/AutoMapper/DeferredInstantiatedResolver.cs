using System;

namespace AutoMapper
{
	internal class DeferredInstantiatedResolver : IValueResolver
	{
		private readonly Func<IValueResolver> _constructor;
		private IValueResolver _resolver;

		public DeferredInstantiatedResolver(Func<IValueResolver> constructor)
		{
			_constructor = constructor;
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
			CreateResolver();
			return _resolver.Resolve(source);
		}

		private void CreateResolver()
		{
			if (_resolver == null)
			{
				_resolver = _constructor();
			}
		}
	}
}