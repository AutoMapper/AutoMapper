using System;
using System.ComponentModel;

namespace AutoMapper.Internal
{
    public class NullableConverterFactory : INullableConverterFactory
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

            public Type UnderlyingType { get { return _nullableConverter.UnderlyingType; } }
        }
    }
}
