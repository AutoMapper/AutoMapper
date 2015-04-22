using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Internal
{
    internal class ProbingAdapterResolver : IAdapterResolver
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Type, object> _adapters = new Dictionary<Type, object>();

        public ProbingAdapterResolver(string[] ignored)
        {

        }

        public object Resolve(Type type)
        {
            lock (_lock)
            {
                object instance;
                if (!_adapters.TryGetValue(type, out instance))
                {
                    instance = ResolveAdapter(type);
                    _adapters.Add(type, instance);
                }

                return instance;
            }
        }

        private static object ResolveAdapter(Type interfaceType)
        {
            string typeName = MakeAdapterTypeName(interfaceType);

            Type type;
            Assembly assembly = typeof(ProbingAdapterResolver).Assembly();

            // Is there an override?
            type = assembly.GetType(typeName + "Override");
            if (type != null)
                return Activator.CreateInstance(type);

            // Fall back to a default implementation
            type = assembly.GetType(typeName);
            if (type != null)
                return Activator.CreateInstance(type);

            return null;
        }

        private static string MakeAdapterTypeName(Type interfaceType)
        {
            // For example, if we're looking for an implementation of System.Security.Cryptography.ICryptographyFactory, 
            // then we'll look for System.Security.Cryptography.CryptographyFactory
            return interfaceType.Namespace + "." + interfaceType.Name.Substring(1);
        }
    }
}
