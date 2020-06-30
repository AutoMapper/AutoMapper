using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace AutoMapper.Execution
{
    public static class ProxyGenerator
    {
        private static readonly byte[] privateKey =
            StringToByteArray(
                "002400000480000094000000060200000024000052534131000400000100010079dfef85ed6ba841717e154f13182c0a6029a40794a6ecd2886c7dc38825f6a4c05b0622723a01cd080f9879126708eef58f134accdc99627947425960ac2397162067507e3c627992aa6b92656ad3380999b30b5d5645ba46cc3fcc6a1de5de7afebcf896c65fb4f9547a6c0c6433045fceccb1fa15e960d519d0cd694b29a4");

        private static readonly byte[] privateKeyToken = StringToByteArray("be96cd2c38ef1005");

        private static readonly MethodInfo delegate_Combine = typeof(Delegate).GetRuntimeMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });

        private static readonly MethodInfo delegate_Remove = typeof(Delegate).GetRuntimeMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });

        private static readonly EventInfo iNotifyPropertyChanged_PropertyChanged =
            typeof(INotifyPropertyChanged).GetRuntimeEvent("PropertyChanged");

        private static readonly ConstructorInfo proxyBase_ctor =
            typeof(ProxyBase).GetDeclaredConstructor(Type.EmptyTypes);

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
            var typeName = $"Proxy_{interfaceType.FullName}_{typeDescription.GetHashCode()}_{propertyNames}";
            const int MaxTypeNameLength = 1023;
            typeName = typeName.Substring(0, Math.Min(MaxTypeNameLength, typeName.Length));
            var allInterfaces = new List<Type> { interfaceType };
            allInterfaces.AddRange(interfaceType.GetTypeInfo().ImplementedInterfaces);
            Debug.WriteLine(typeName, "Emitting proxy type");
            TypeBuilder typeBuilder = proxyModule.DefineType(typeName,
                TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof(ProxyBase),
                interfaceType.IsInterface ? new[] { interfaceType } : Type.EmptyTypes);
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes);
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
                if(fieldBuilders.TryGetValue(property.Name, out var propertyEmitter))
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
                        new PropertyEmitter(typeBuilder, property, propertyChangedField));
                }
            }
            return typeBuilder.CreateType();
        }

        public static Type GetProxyType(Type interfaceType)
        {
            var key = new TypeDescription(interfaceType);
            if(!interfaceType.IsInterface)
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
}
