using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Internal
{
    internal class ProbingAdapterResolver : IAdapterResolver
    {
        private readonly string[] _platformNames;
        private readonly Func<string, Assembly> _assemblyLoader;
        private readonly object _lock = new object();
        private readonly Dictionary<Type, object> _adapters = new Dictionary<Type, object>();
        private Assembly _assembly;
        private bool _probed;

        public ProbingAdapterResolver(params string[] platformNames)
            : this(Assembly.Load, platformNames)
        {
        }

        public ProbingAdapterResolver(Func<string, Assembly> assemblyLoader, params string[] platformNames)
        {
            _platformNames = platformNames;
            _assemblyLoader = assemblyLoader;
        }

        public object Resolve(Type type)
        {
            lock (_lock)
            {
                object instance;
                if (!_adapters.TryGetValue(type, out instance))
                {
                    Assembly assembly = GetPlatformSpecificAssembly();
                    if (assembly == null)
                        return null;

                    instance = ResolveAdapter(assembly, type);
                    _adapters.Add(type, instance);
                }

                return instance;
            }
        }

        private static object ResolveAdapter(Assembly assembly, Type interfaceType)
        {
            string typeName = MakeAdapterTypeName(interfaceType);

            try
            {
                // Is there an override?
                Type type = assembly.GetType(typeName + "Override");
                if (type != null)
                    return Activator.CreateInstance(type);

                // Fall back to a default implementation
                type = assembly.GetType(typeName);
                if (type != null)
                    return Activator.CreateInstance(type);


                // Fallback to looking in this assembly for a default
                type = typeof(ProbingAdapterResolver).Assembly.GetType(typeName);
                
                return type != null ? Activator.CreateInstance(type) : null;
            }
            catch
            {
                return null;
            }
        }

        private static string MakeAdapterTypeName(Type interfaceType)
        {
            // For example, if we're looking for an implementation of System.Security.Cryptography.ICryptographyFactory, 
            // then we'll look for System.Security.Cryptography.CryptographyFactory
            return interfaceType.Namespace + "." + interfaceType.Name.Substring(1);
        }

        private Assembly GetPlatformSpecificAssembly()
        {   
            if (_assembly == null && !_probed)
            {
                _probed = true;
                _assembly = ProbeForPlatformSpecificAssembly();
            }

            return _assembly;
        }

        private Assembly ProbeForPlatformSpecificAssembly()
        {
            return _platformNames.
                Select(ProbeForPlatformSpecificAssembly)
                .FirstOrDefault(assembly => assembly != null);
        }

        private Assembly ProbeForPlatformSpecificAssembly(string platformName)
        {
            var assemblyName = new AssemblyName(GetType().Assembly.FullName)
            {
                Name = "AutoMapper." + platformName
            };

            try
            {
                return _assemblyLoader(assemblyName.ToString());
            }
            catch (FileNotFoundException)
            {
            }

            return null;
        }
    }
}
