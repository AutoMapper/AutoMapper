namespace AutoMapper.Internal
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public static class PlatformAdapter
    {
        //TODO: could possibly make this read only
        /// <summary>
        /// 
        /// </summary>
        private static System.Collections.Generic.IDictionary<Type, Func<object>> Factories { get; }
            = new System.Collections.Generic.Dictionary<Type, Func<object>>
            {
                {typeof(IMapperContext), () => new MapperContext()},
                {typeof(IMapperContextFactory), () => new MapperContextFactory()},
#if NET4 || NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || DNXCORE50
                {typeof (IDictionaryFactory), () => new ConcurrentDictionaryFactory()},
#else
                {typeof(IDictionaryFactory), () => new DictionaryFactory()},
#endif
#if NET4 || NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || DNXCORE50
                {typeof (IEnumNameValueMapperFactory), () => new EnumNameValueMapperFactoryOverride()},
#else
                {typeof(IEnumNameValueMapperFactory), () => new EnumNameValueMapperFactory() },
#endif
#if MONODROID || MONOTOUCH || __IOS__ || NET4
                {typeof (INullableConverterFactory), () => new NullableConverterFactoryOverride()},
#else
                {typeof(INullableConverterFactory), () => new NullableConverterFactory()},
#endif
#if MONODROID || NET4
                {typeof (IProxyGeneratorFactory), () => new ProxyGeneratorFactoryOverride()},
#else
                {typeof(IProxyGeneratorFactory), () => new ProxyGeneratorFactory()},
#endif
#if MONODROID || MONOTOUCH || __IOS__ || NETFX_CORE || NET4
                {typeof (IReaderWriterLockSlimFactory), () => new ReaderWriterLockSlimFactoryOverride()},
#else
                {typeof(IReaderWriterLockSlimFactory), () => new ReaderWriterLockSlimFactory()},
#endif
            };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            var value = (T) Factories[typeof (T)]();

            return value;
        }
    }
}