namespace AutoMapper.Internal
{
    using System;

    public class EnumNameValueMapperFactory : IEnumNameValueMapperFactory
    {
        public IEnumNameValueMapper Create()
        {
            return new EnumVameValueMapper();
        }

        private class EnumVameValueMapper : IEnumNameValueMapper
        {
            public bool IsMatch(Type enumDestinationType, string sourceValue)
            {
                return false;
            }

            public object Convert(Type enumSourceType, Type enumDestinationType, ResolutionContext context)
            {
                throw new PlatformNotSupportedException("Mapping enum names to values not supported on this platform.");
            }
        }
    }
}