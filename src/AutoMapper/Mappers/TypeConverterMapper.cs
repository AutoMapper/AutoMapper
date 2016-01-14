using System.Collections.Generic;
using System.Collections.ObjectModel;

#if !PORTABLE
namespace AutoMapper.Mappers
{
    using System;
    using System.ComponentModel;
    using Internal;

    public class TypeConverterMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            if (context.SourceValue == null)
            {
                return context.Engine.CreateObject(context);
            }
            Func<object> converter = GetConverter(context);
            return converter?.Invoke();
        }

        private static Func<object> GetConverter(ResolutionContext context)
        {
            TypeConverter typeConverter = GetTypeConverter(context.SourceType);
            if (typeConverter.CanConvertTo(context.DestinationType))
                return () => typeConverter.ConvertTo(context.SourceValue, context.DestinationType);
            if (context.DestinationType.IsNullableType() &&
                typeConverter.CanConvertTo(Nullable.GetUnderlyingType(context.DestinationType)))
                return
                    () =>
                        typeConverter.ConvertTo(context.SourceValue, Nullable.GetUnderlyingType(context.DestinationType));

            typeConverter = GetTypeConverter(context.DestinationType);
            if (typeConverter.CanConvertFrom(context.SourceType))
                return () => typeConverter.ConvertFrom(context.SourceValue);

            return null;
        }

        public bool IsMatch(TypePair context)
        {
            var sourceTypeConverter = GetTypeConverter(context.SourceType);
            var destTypeConverter = GetTypeConverter(context.DestinationType);

            return sourceTypeConverter.CanConvertTo(context.DestinationType) ||
                   (context.DestinationType.IsNullableType() &&
                    sourceTypeConverter.CanConvertTo(Nullable.GetUnderlyingType(context.DestinationType)) ||
                    destTypeConverter.CanConvertFrom(context.SourceType));
        }

        private static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }
    }
}
#else
namespace AutoMapper.Mappers
{
    using System;
    using System.ComponentModel;
    using Internal;

    public class TypeConverterMapper : IObjectMapper
    {
        private IReadOnlyDictionary<TypePair, Func<object, object>> _converters = new ReadOnlyDictionary<TypePair, Func<object, object>>(new Dictionary<TypePair, Func<object, object>>
        {
            { new TypePair(typeof(byte), typeof(bool)), foo => Convert.ToBoolean((byte)foo) },
        });  

        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            if (context.SourceValue == null)
            {
                return mapper.CreateObject(context);
            }
            Func<object> converter = GetConverter(context);
            return converter?.Invoke();
        }

        public bool IsMatch(ResolutionContext context)
        {
            return _converters.ContainsKey(context)
        }

        private static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }
    }
}
#endif