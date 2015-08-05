namespace AutoMapper.Internal
{
    using System;
    using System.Reflection;

    public class DeferredInstantiatedConverter : ITypeConverter<object, object>
    {
        private readonly Func<ResolutionContext, object> _instantiator;
        private readonly MethodInfo _converterMethod;

        public DeferredInstantiatedConverter(Type typeConverterType, Func<ResolutionContext, object> instantiator)
        {
            _instantiator = instantiator;
            _converterMethod = typeConverterType.GetMethod("Convert");
        }

        public object Convert(ResolutionContext context)
        {
            var converter = _instantiator(context);
            var method = _converterMethod.ContainsGenericParameters ? GetClosedConvertMethod(context) : _converterMethod;
            return method.Invoke(converter, new[] { context });
        }

        private MethodInfo GetClosedConvertMethod(ResolutionContext context)
        {
            var interfaceType = typeof(ITypeConverter<,>).MakeGenericType(context.SourceType, context.DestinationType);
            return interfaceType.GetMethod("Convert");
        }
    }

    public class DeferredInstantiatedConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination>
    {
        private readonly Func<ResolutionContext, ITypeConverter<TSource, TDestination>> _instantiator;

        public DeferredInstantiatedConverter(Func<ResolutionContext, ITypeConverter<TSource, TDestination>> instantiator)
        {
            _instantiator = instantiator;
        }

        public TDestination Convert(ResolutionContext context)
        {
            var typeConverter = _instantiator(context);

            return typeConverter.Convert(context);
        }
    }
}