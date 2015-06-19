#if NET4 || MONODROID || MONOTOUCH || __IOS__ || SILVERLIGHT || DNXCORE50

namespace AutoMapper.Mappers
{
    using System;
    using System.ComponentModel;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class TypeConverterMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var runner = context.MapperContext.Runner;
            if (context.SourceValue == null)
            {
                return runner.CreateObject(context);
            }
            var converter = GetConverter(context);
            return converter?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Func<object> GetConverter(ResolutionContext context)
        {
            var typeConverter = GetTypeConverter(context.SourceType);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return GetConverter(context) != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static TypeConverter GetTypeConverter(Type type)
        {
#if !SILVERLIGHT
            return TypeDescriptor.GetConverter(type);
#else
            var attributes = type.GetCustomAttributes(typeof (TypeConverterAttribute), false);

            if (attributes.Length != 1)
                return new TypeConverter();

            var converterAttribute = (TypeConverterAttribute) attributes[0];
            var converterType = Type.GetType(converterAttribute.ConverterTypeName);

            if (converterType == null)
                return new TypeConverter();

            return Activator.CreateInstance(converterType) as TypeConverter;
#endif
        }
    }
}

#endif