using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper.Execution
{
    public static class ProxyGenerator
    {
        private static readonly byte[] PrivateKey = StringToByteArray(
                "002400000480000094000000060200000024000052534131000400000100010079dfef85ed6ba841717e154f13182c0a6029a40794a6ecd2886c7dc38825f6a4c05b0622723a01cd080f9879126708eef58f134accdc99627947425960ac2397162067507e3c627992aa6b92656ad3380999b30b5d5645ba46cc3fcc6a1de5de7afebcf896c65fb4f9547a6c0c6433045fceccb1fa15e960d519d0cd694b29a4");
        private static readonly byte[] PrivateKeyToken = StringToByteArray("be96cd2c38ef1005");
        private static readonly MethodInfo DelegateCombine = typeof(Delegate).GetMethod(nameof(Delegate.Combine), new[] { typeof(Delegate), typeof(Delegate) });
        private static readonly MethodInfo DelegateRemove = typeof(Delegate).GetMethod(nameof(Delegate.Remove));
        private static readonly EventInfo PropertyChanged = typeof(INotifyPropertyChanged).GetEvent(nameof(INotifyPropertyChanged.PropertyChanged));
        private static readonly ConstructorInfo ProxyBaseCtor = typeof(ProxyBase).GetConstructor(Type.EmptyTypes);
        private static readonly ModuleBuilder ProxyModule = CreateProxyModule();
        private static readonly LockingConcurrentDictionary<TypeDescription, Type> ProxyTypes = new LockingConcurrentDictionary<TypeDescription, Type>(EmitProxy);
        private static ModuleBuilder CreateProxyModule()
        {
            var name = new AssemblyName("AutoMapper.Proxies");
            name.SetPublicKey(PrivateKey);
            name.SetPublicKeyToken(PrivateKeyToken);
            var builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            return builder.DefineDynamicModule("AutoMapper.Proxies.emit");
        }
        private static Type EmitProxy(TypeDescription typeDescription)
        {
            var interfaceType = typeDescription.Type;
            var additionalProperties = typeDescription.AdditionalProperties;
            var typeBuilder = GenerateType();
            GenerateConstructor();
            FieldBuilder propertyChangedField = null;
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(interfaceType))
            {
                GeneratePropertyChanged();
            }
            GenerateFields();
            return typeBuilder.CreateType();
            TypeBuilder GenerateType()
            {
                var propertyNames = string.Join("_", additionalProperties.Select(p => p.Name));
                var typeName = $"Proxy_{interfaceType.FullName}_{typeDescription.GetHashCode()}_{propertyNames}";
                const int MaxTypeNameLength = 1023;
                typeName = typeName.Substring(0, Math.Min(MaxTypeNameLength, typeName.Length));
                Debug.WriteLine(typeName, "Emitting proxy type");
                return ProxyModule.DefineType(typeName,
                    TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof(ProxyBase),
                    interfaceType.IsInterface ? new[] { interfaceType } : Type.EmptyTypes);
            }
            void GeneratePropertyChanged()
            {
                propertyChangedField = typeBuilder.DefineField(PropertyChanged.Name, typeof(PropertyChangedEventHandler), FieldAttributes.Private);
                EventAccessor(PropertyChanged.AddMethod, DelegateCombine);
                EventAccessor(PropertyChanged.RemoveMethod, DelegateRemove);
            }
            void EventAccessor(MethodInfo method, MethodInfo delegateMethod)
            {
                var eventAccessor = typeBuilder.DefineMethod(method.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual, typeof(void),
                    new[] { typeof(PropertyChangedEventHandler) });
                var addIl = eventAccessor.GetILGenerator();
                addIl.Emit(OpCodes.Ldarg_0);
                addIl.Emit(OpCodes.Dup);
                addIl.Emit(OpCodes.Ldfld, propertyChangedField);
                addIl.Emit(OpCodes.Ldarg_1);
                addIl.Emit(OpCodes.Call, delegateMethod);
                addIl.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
                addIl.Emit(OpCodes.Stfld, propertyChangedField);
                addIl.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(eventAccessor, method);
            }
            void GenerateFields()
            {
                var fieldBuilders = new Dictionary<string, PropertyEmitter>();
                foreach (var property in PropertiesToImplement())
                {
                    if (fieldBuilders.TryGetValue(property.Name, out var propertyEmitter))
                    {
                        if (propertyEmitter.PropertyType != property.Type && (property.CanWrite || !property.Type.IsAssignableFrom(propertyEmitter.PropertyType)))
                        {
                            throw new ArgumentException($"The interface has a conflicting property {property.Name}", nameof(interfaceType));
                        }
                    }
                    else
                    {
                        fieldBuilders.Add(property.Name, new PropertyEmitter(typeBuilder, property, propertyChangedField));
                    }
                }
            }
            List<PropertyDescription> PropertiesToImplement()
            {
                var propertiesToImplement = new List<PropertyDescription>();
                var allInterfaces = new List<Type>(interfaceType.GetInterfaces()) { interfaceType };
                // first we collect all properties, those with setters before getters in order to enable less specific redundant getters
                foreach (var property in
                    allInterfaces.Where(intf => intf != typeof(INotifyPropertyChanged))
                        .SelectMany(intf => intf.GetProperties())
                        .Select(p => new PropertyDescription(p))
                        .Concat(additionalProperties))
                {
                    if (property.CanWrite)
                    {
                        propertiesToImplement.Insert(0, property);
                    }
                    else
                    {
                        propertiesToImplement.Add(property);
                    }
                }
                return propertiesToImplement;
            }
            void GenerateConstructor()
            {
                var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                var ctorIl = constructorBuilder.GetILGenerator();
                ctorIl.Emit(OpCodes.Ldarg_0);
                ctorIl.Emit(OpCodes.Call, ProxyBaseCtor);
                ctorIl.Emit(OpCodes.Ret);
            }
        }
        public static Type GetProxyType(Type interfaceType) => interfaceType.IsInterface ?
            ProxyTypes.GetOrAdd(new TypeDescription(interfaceType)) : throw new ArgumentException("Only interfaces can be proxied", nameof(interfaceType));
        public static Type GetSimilarType(Type sourceType, IEnumerable<PropertyDescription> additionalProperties) =>
            ProxyTypes.GetOrAdd(new TypeDescription(sourceType, additionalProperties));
        private static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        class PropertyEmitter
        {
            private static readonly MethodInfo ProxyBaseNotifyPropertyChanged =
                typeof(ProxyBase).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "NotifyPropertyChanged");
            private readonly FieldBuilder _fieldBuilder;
            private readonly MethodBuilder _getterBuilder;
            private readonly PropertyBuilder _propertyBuilder;
            private readonly MethodBuilder _setterBuilder;
            public PropertyEmitter(TypeBuilder owner, PropertyDescription property, FieldBuilder propertyChangedField)
            {
                var name = property.Name;
                var propertyType = property.Type;
                _fieldBuilder = owner.DefineField($"<{name}>", propertyType, FieldAttributes.Private);
                _propertyBuilder = owner.DefineProperty(name, PropertyAttributes.None, propertyType, null);
                _getterBuilder = owner.DefineMethod($"get_{name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName, propertyType, Type.EmptyTypes);
                ILGenerator getterIl = _getterBuilder.GetILGenerator();
                getterIl.Emit(OpCodes.Ldarg_0);
                getterIl.Emit(OpCodes.Ldfld, _fieldBuilder);
                getterIl.Emit(OpCodes.Ret);
                _propertyBuilder.SetGetMethod(_getterBuilder);
                if (!property.CanWrite)
                {
                    return;
                }
                _setterBuilder = owner.DefineMethod($"set_{name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName, typeof(void), new[] { propertyType });
                ILGenerator setterIl = _setterBuilder.GetILGenerator();
                setterIl.Emit(OpCodes.Ldarg_0);
                setterIl.Emit(OpCodes.Ldarg_1);
                setterIl.Emit(OpCodes.Stfld, _fieldBuilder);
                if (propertyChangedField != null)
                {
                    setterIl.Emit(OpCodes.Ldarg_0);
                    setterIl.Emit(OpCodes.Dup);
                    setterIl.Emit(OpCodes.Ldfld, propertyChangedField);
                    setterIl.Emit(OpCodes.Ldstr, name);
                    setterIl.Emit(OpCodes.Call, ProxyBaseNotifyPropertyChanged);
                }
                setterIl.Emit(OpCodes.Ret);
                _propertyBuilder.SetSetMethod(_setterBuilder);
            }
            public Type PropertyType => _propertyBuilder.PropertyType;
            public MethodBuilder GetGetter(Type requiredType)
                => !requiredType.IsAssignableFrom(PropertyType)
                ? throw new InvalidOperationException("Types are not compatible")
                : _getterBuilder;
            public MethodBuilder GetSetter(Type requiredType)
                => !PropertyType.IsAssignableFrom(requiredType)
                ? throw new InvalidOperationException("Types are not compatible")
                : _setterBuilder;
        }
    }
    public abstract class ProxyBase
    {
        public ProxyBase() { }
        protected void NotifyPropertyChanged(PropertyChangedEventHandler handler, string method) =>
            handler?.Invoke(this, new PropertyChangedEventArgs(method));
    }
    public readonly struct TypeDescription : IEquatable<TypeDescription>
    {
        public readonly Type Type;
        public readonly PropertyDescription[] AdditionalProperties;
        public TypeDescription(Type type) : this(type, Array.Empty<PropertyDescription>()) { }
        public TypeDescription(Type type, IEnumerable<PropertyDescription> additionalProperties)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            if (additionalProperties == null)
            {
                throw new ArgumentNullException(nameof(additionalProperties));
            }
            AdditionalProperties = additionalProperties.OrderBy(p => p.Name).ToArray();
        }
        public override int GetHashCode()
        {
            var hashCode = Type.GetHashCode();
            foreach (var property in AdditionalProperties)
            {
                hashCode = HashCodeCombiner.CombineCodes(hashCode, property.GetHashCode());
            }
            return hashCode;
        }
        public override bool Equals(object other) => other is TypeDescription description && Equals(description);
        public bool Equals(TypeDescription other) => Type == other.Type && AdditionalProperties.SequenceEqual(other.AdditionalProperties);
        public static bool operator ==(in TypeDescription left, in TypeDescription right) => left.Equals(right);
        public static bool operator !=(in TypeDescription left, in TypeDescription right) => !left.Equals(right);
    }
    [DebuggerDisplay("{Name}-{Type.Name}")]
    public readonly struct PropertyDescription : IEquatable<PropertyDescription>
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly bool CanWrite;
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
        public override int GetHashCode()
        {
            var code = HashCodeCombiner.Combine(Name, Type);
            return HashCodeCombiner.CombineCodes(code, CanWrite.GetHashCode());
        }
        public override bool Equals(object other) => other is PropertyDescription description && Equals(description);
        public bool Equals(PropertyDescription other) => Name == other.Name && Type == other.Type && CanWrite == other.CanWrite;
        public static bool operator ==(in PropertyDescription left, in PropertyDescription right) => left.Equals(right);
        public static bool operator !=(in PropertyDescription left, in PropertyDescription right) => !left.Equals(right);
    }
}