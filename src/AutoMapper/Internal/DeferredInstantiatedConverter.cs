using System;
using System.Reflection;

namespace AutoMapper
{
	internal class DeferredInstantiatedConverter
	{
		private readonly Func<object> _instantiator;
		private readonly MethodInfo _converterMethod;
		private object _customTypeConverter;

		public DeferredInstantiatedConverter(Type typeConverterType, Func<object> instantiator)
		{
			_instantiator = instantiator;
			_converterMethod = typeConverterType.GetMethod("Convert");
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