using System;
using System.Reflection;

namespace AutoMapper
{
	internal class DeferredInstantiatedConverter
	{
		private readonly Func<object> _instantiator;
		private readonly MethodInfo _converterMethod;

		public DeferredInstantiatedConverter(Type typeConverterType, Func<object> instantiator)
		{
			_instantiator = instantiator;
			_converterMethod = typeConverterType.GetMethod("Convert");
		}

		public object Convert(object source)
		{
		    var converter = _instantiator();

		    return _converterMethod.Invoke(converter, new[] { source });
		}
	}
	internal class DeferredInstantiatedConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination>
	{
		private readonly Func<ITypeConverter<TSource, TDestination>> _instantiator;

		public DeferredInstantiatedConverter(Func<ITypeConverter<TSource, TDestination>> instantiator)
		{
			_instantiator = instantiator;
		}

		public TDestination Convert(TSource source)
		{
		    var typeConverter = _instantiator();

		    return typeConverter.Convert(source);
		}
	}
}