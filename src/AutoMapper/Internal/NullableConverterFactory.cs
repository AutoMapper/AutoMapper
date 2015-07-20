#if MONODROID || MONOTOUCH || __IOS__ || NET4
namespace AutoMapper.Internal
{
    using System;
    using System.ComponentModel;

    public class NullableConverterFactoryOverride : INullableConverterFactory
    {
        public INullableConverter Create(Type nullableType)
        {
            return new NullableConverterImpl(new NullableConverter(nullableType));
        }

        private class NullableConverterImpl : INullableConverter
        {
            private readonly NullableConverter _nullableConverter;

            public NullableConverterImpl(NullableConverter nullableConverter)
            {
                _nullableConverter = nullableConverter;
            }

            public object ConvertFrom(object value)
            {
                return _nullableConverter.ConvertFrom(value);
            }

            public Type UnderlyingType => _nullableConverter.UnderlyingType;
        }
    }
}

#endif