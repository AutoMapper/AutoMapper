using System;

namespace AutoMapper
{
	internal class DeferredInstantiatedConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination>
	{
		private readonly Func<ITypeConverter<TSource, TDestination>> _instantiator;
		private ITypeConverter<TSource, TDestination> _customTypeConverter;

		public DeferredInstantiatedConverter(Func<ITypeConverter<TSource, TDestination>> instantiator)
		{
			_instantiator = instantiator;
		}

		private void Initialize()
		{
			if (_customTypeConverter == null)
			{
				_customTypeConverter = _instantiator();
			}
		}

		public TDestination Convert(TSource source)
		{
			Initialize();

			return _customTypeConverter.Convert(source);
		}
	}
}