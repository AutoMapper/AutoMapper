namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;

    public static class ProxyGenerator
    {
        private static readonly byte[] privateKey =
            StringToByteArray(
                "002400000480000094000000060200000024000052534131000400000100010079dfef85ed6ba841717e154f13182c0a6029a40794a6ecd2886c7dc38825f6a4c05b0622723a01cd080f9879126708eef58f134accdc99627947425960ac2397162067507e3c627992aa6b92656ad3380999b30b5d5645ba46cc3fcc6a1de5de7afebcf896c65fb4f9547a6c0c6433045fceccb1fa15e960d519d0cd694b29a4");

        private static readonly byte[] privateKeyToken = StringToByteArray("be96cd2c38ef1005");

        private static readonly MethodInfo delegate_Combine = typeof(Delegate).GetDeclaredMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });

        private static readonly MethodInfo delegate_Remove = typeof(Delegate).GetDeclaredMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });

        private static readonly EventInfo iNotifyPropertyChanged_PropertyChanged =
            typeof(INotifyPropertyChanged).GetRuntimeEvent("PropertyChanged");

        private static readonly ConstructorInfo proxyBase_ctor =
            typeof(ProxyBase).GetDeclaredConstructor(new Type[0]);

        private static readonly ModuleBuilder proxyModule = CreateProxyModule();

        private static readonly LockingConcurrentDictionary<TypeDescription, Type> proxyTypes = new LockingConcurrentDictionary<TypeDescription, Type>(EmitProxy);

        private static ModuleBuilder CreateProxyModule()
        {
            AssemblyName name = new AssemblyName("AutoMapper.Proxies");
            name.SetPublicKey(privateKey);
            name.SetPublicKeyToken(privateKeyToken);

            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);

            return builder.DefineDynamicModule("AutoMapper.Proxies.emit");
        }

        private static Type EmitProxy(TypeDescription typeDescription)
        {
            var interfaceType = typeDescription.Type;
            var additionalProperties = typeDescription.AdditionalProperties;
            var propertyNames = string.Join("_", additionalProperties.Select(p => p.Name));
            var typeName = $"Proxy_{interfaceType.FullName}_{propertyNames}_{typeDescription.GetHashCode()}";
            var allInterfaces = new List<Type> { interfaceType };
            allInterfaces.AddRange(interfaceType.GetTypeInfo().ImplementedInterfaces);
            Debug.WriteLine(typeName, "Emitting proxy type");
            TypeBuilder typeBuilder = proxyModule.DefineType(typeName,
                TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof(ProxyBase),
                interfaceType.IsInterface() ? new[] { interfaceType } : new Type[0]);
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard, new Type[0]);
            ILGenerator ctorIl = constructorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, proxyBase_ctor);
            ctorIl.Emit(OpCodes.Ret);
            FieldBuilder propertyChangedField = null;
            if(typeof(INotifyPropertyChanged).IsAssignableFrom(interfaceType))
            {
                propertyChangedField = typeBuilder.DefineField("PropertyChanged", typeof(PropertyChangedEventHandler),
                    FieldAttributes.Private);
                MethodBuilder addPropertyChangedMethod = typeBuilder.DefineMethod("add_PropertyChanged",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual, typeof(void),
                    new[] { typeof(PropertyChangedEventHandler) });
                ILGenerator addIl = addPropertyChangedMethod.GetILGenerator();
                addIl.Emit(OpCodes.Ldarg_0);
                addIl.Emit(OpCodes.Dup);
                addIl.Emit(OpCodes.Ldfld, propertyChangedField);
                addIl.Emit(OpCodes.Ldarg_1);
                addIl.Emit(OpCodes.Call, delegate_Combine);
                addIl.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
                addIl.Emit(OpCodes.Stfld, propertyChangedField);
                addIl.Emit(OpCodes.Ret);
                MethodBuilder removePropertyChangedMethod = typeBuilder.DefineMethod("remove_PropertyChanged",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual, typeof(void),
                    new[] { typeof(PropertyChangedEventHandler) });
                ILGenerator removeIl = removePropertyChangedMethod.GetILGenerator();
                removeIl.Emit(OpCodes.Ldarg_0);
                removeIl.Emit(OpCodes.Dup);
                removeIl.Emit(OpCodes.Ldfld, propertyChangedField);
                removeIl.Emit(OpCodes.Ldarg_1);
                removeIl.Emit(OpCodes.Call, delegate_Remove);
                removeIl.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
                removeIl.Emit(OpCodes.Stfld, propertyChangedField);
                removeIl.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(addPropertyChangedMethod,
                    iNotifyPropertyChanged_PropertyChanged.GetAddMethod());
                typeBuilder.DefineMethodOverride(removePropertyChangedMethod,
                    iNotifyPropertyChanged_PropertyChanged.GetRemoveMethod());
            }
            var propertiesToImplement = new List<PropertyDescription>();
            // first we collect all properties, those with setters before getters in order to enable less specific redundant getters
            foreach(var property in
                allInterfaces.Where(intf => intf != typeof(INotifyPropertyChanged))
                    .SelectMany(intf => intf.GetProperties())
                    .Select(p => new PropertyDescription(p))
                    .Concat(additionalProperties))
            {
                if(property.CanWrite)
                {
                    propertiesToImplement.Insert(0, property);
                }
                else
                {
                    propertiesToImplement.Add(property);
                }
            }
            var fieldBuilders = new Dictionary<string, PropertyEmitter>();
            foreach(var property in propertiesToImplement)
            {
                PropertyEmitter propertyEmitter;
                if(fieldBuilders.TryGetValue(property.Name, out propertyEmitter))
                {
                    if((propertyEmitter.PropertyType != property.Type) &&
                        ((property.CanWrite) || (!property.Type.IsAssignableFrom(propertyEmitter.PropertyType))))
                    {
                        throw new ArgumentException(
                            $"The interface has a conflicting property {property.Name}",
                            nameof(interfaceType));
                    }
                }
                else
                {
                    fieldBuilders.Add(property.Name,
                        propertyEmitter =
                            new PropertyEmitter(typeBuilder, property, propertyChangedField));
                }
            }
            return typeBuilder.CreateType();
        }

        public static Type GetProxyType(Type interfaceType)
        {
            var key = new TypeDescription(interfaceType);
            if(!interfaceType.IsInterface())
            {
                throw new ArgumentException("Only interfaces can be proxied", nameof(interfaceType));
            }
            return proxyTypes.GetOrAdd(key);
        }

        public static Type GetSimilarType(Type sourceType, IEnumerable<PropertyDescription> additionalProperties)
        {
            return proxyTypes.GetOrAdd(new TypeDescription(sourceType, additionalProperties));
        }

        private static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for(int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }

    public struct TypeDescription : IEquatable<TypeDescription>
    {
        public TypeDescription(Type type) : this(type, PropertyDescription.Empty)
        {
        }

        public TypeDescription(Type type, IEnumerable<PropertyDescription> additionalProperties)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            if(additionalProperties == null)
            {
                throw new ArgumentNullException(nameof(additionalProperties));
            }
            AdditionalProperties = additionalProperties.OrderBy(p => p.Name).ToArray();
        }

        public Type Type { get; }

        public PropertyDescription[] AdditionalProperties { get; }

        public override int GetHashCode()
        {
            var hashCode = Type.GetHashCode();
            foreach(var property in AdditionalProperties)
            {
                hashCode = HashCodeCombiner.CombineCodes(hashCode, property.GetHashCode());
            }
            return hashCode;
        }

        public override bool Equals(object other) => other is TypeDescription && Equals((TypeDescription)other);

        public bool Equals(TypeDescription other) => Type == other.Type && AdditionalProperties.SequenceEqual(other.AdditionalProperties);

        public static bool operator ==(TypeDescription left, TypeDescription right) => left.Equals(right);

        public static bool operator !=(TypeDescription left, TypeDescription right) => !left.Equals(right);
    }

    [DebuggerDisplay("{Name}-{Type.Name}")]
    public struct PropertyDescription : IEquatable<PropertyDescription>
    {
        internal static PropertyDescription[] Empty = new PropertyDescription[0];

        public PropertyDescription(string name, Type type, bool canWrite = true)
        {
            Name = name;
            Type = type;
            CanWrite = canWrite;
        }

        public PropertyDescription(PropertyInfo property)
        {
            Name = property.Name;
            Type = property.PropertyType;
            CanWrite = property.CanWrite;
        }

        public string Name { get; }

        public Type Type { get; }

        public bool CanWrite { get; }

        public override int GetHashCode()
        {
            var code = HashCodeCombiner.Combine(Name, Type);
            return HashCodeCombiner.CombineCodes(code, CanWrite.GetHashCode());
        }

        public override bool Equals(object other) => other is PropertyDescription && Equals((PropertyDescription)other);

        public bool Equals(PropertyDescription other) => Name == other.Name && Type == other.Type && CanWrite == other.CanWrite;

        public static bool operator ==(PropertyDescription left, PropertyDescription right) => left.Equals(right);

        public static bool operator !=(PropertyDescription left, PropertyDescription right) => !left.Equals(right);
    }
}