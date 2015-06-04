namespace AutoMapper.Internal
{
    using System;

    public class NullableConverterFactory : INullableConverterFactory
    {
        public INullableConverter Create(Type nullableType)
        {
            return new NullableConverterImpl(nullableType);
        }

        private class NullableConverterImpl : INullableConverter
        {
            private readonly Type _nullableType;

            public NullableConverterImpl(Type nullableType)
            {
                _nullableType = nullableType;
                UnderlyingType = Nullable.GetUnderlyingType(_nullableType);
            }

            public object ConvertFrom(object value)
            {
                if (value == null)
                    return Activator.CreateInstance(_nullableType);

                if (value.GetType() == UnderlyingType)
                    return Activator.CreateInstance(_nullableType, value);

                return Activator.CreateInstance(_nullableType, Convert.ChangeType(value, UnderlyingType, null));
            }

            public Type UnderlyingType { get; }
        }
    }
}