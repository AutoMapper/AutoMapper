using System;
using System.Reflection;

namespace AutoMapper
{
	internal class DeferredInstantiatedConverter<TTypeConverter>
	{
		private readonly Func<TTypeConverter> _instantiator;
		private MethodInfo _converterMethod;
		private object _customTypeConverter;

		public DeferredInstantiatedConverter(Func<TTypeConverter> instantiator)
		{
			_instantiator = instantiator;
			_converterMethod = typeof (TTypeConverter).GetMethod("Convert");
		}

		private void Initialize()
		{
			if (_customTypeConverter == null)
			{
				_customTypeConverter = _instantiator();
			}
		}

		public object Convert(object source)
		{
			Initialize();

			return _converterMethod.Invoke(_customTypeConverter, new[] {source});
		}
	}
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