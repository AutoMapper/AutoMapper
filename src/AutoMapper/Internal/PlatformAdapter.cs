﻿using System;

namespace AutoMapper.Internal
{
    public static class PlatformAdapter
    {
        private static readonly string[] KnownPlatformNames = new[] { "Net4", "WinRT", "SL4", "WP75" };
        private static IAdapterResolver _resolver = new ProbingAdapterResolver(KnownPlatformNames);

        public static T Resolve<T>(bool throwIfNotFound = true)
        {
            var value = (T)_resolver.Resolve(typeof(T));

            if (value == null && throwIfNotFound)
                throw new PlatformNotSupportedException("This type is not supported on this platform " + typeof(T).Name);

            return value;
        }

        // Unit testing helper
        internal static void SetResolver(IAdapterResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
