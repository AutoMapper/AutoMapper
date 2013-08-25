using System;
using System.Linq;

namespace AutoMapper.Internal
{
    public class EnumNameValueMapperFactoryOverride : IEnumNameValueMapperFactory
    {
        public IEnumNameValueMapper Create()
        {
            return new EnumVameValueMapper();
        }

        private class EnumVameValueMapper : IEnumNameValueMapper
        {
            public bool IsMatch(Type enumDestinationType, string sourceValue)
            {
                return !Enum.GetNames(enumDestinationType).Contains(sourceValue);
            }

            public object Convert(Type enumSourceType, Type enumDestinationType, ResolutionContext context)
            {
                Type underlyingSourceType = Enum.GetUnderlyingType(enumSourceType);
                var underlyingSourceValue = System.Convert.ChangeType(context.SourceValue, underlyingSourceType);

                return Enum.ToObject(context.DestinationType, underlyingSourceValue);
            }
        }
    }
}
