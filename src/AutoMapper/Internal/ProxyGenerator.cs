#if MONODROID || NET4
namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;

    public class ProxyGenerator : IProxyGenerator
    {
        private static readonly byte[] privateKey =
            StringToByteArray(
                "002400000480000094000000060200000024000052534131000400000100010079dfef85ed6ba841717e154f13182c0a6029a40794a6ecd2886c7dc38825f6a4c05b0622723a01cd080f9879126708eef58f134accdc99627947425960ac2397162067507e3c627992aa6b92656ad3380999b30b5d5645ba46cc3fcc6a1de5de7afebcf896c65fb4f9547a6c0c6433045fceccb1fa15e960d519d0cd694b29a4");

        private static readonly byte[] privateKeyToken = StringToByteArray("be96cd2c38ef1005");

        private static readonly MethodInfo delegate_Combine = typeof (Delegate).GetMethod("Combine",
            BindingFlags.Public | BindingFlags.Static, null, new[] {typeof (Delegate), typeof (Delegate)}, null);

        private static readonly MethodInfo delegate_Remove = typeof (Delegate).GetMethod("Remove",
            BindingFlags.Public | BindingFlags.Static, null, new[] {typeof (Delegate), typeof (Delegate)}, null);

        private static readonly EventInfo iNotifyPropertyChanged_PropertyChanged =
            typeof (INotifyPropertyChanged).GetEvent("PropertyChanged", BindingFlags.Instance | BindingFlags.Public);

        private static readonly ConstructorInfo proxyBase_ctor =
            typeof (ProxyBase).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,
                Type.EmptyTypes, null);

        private static readonly ModuleBuilder proxyModule = CreateProxyModule();

        private static readonly Dictionary<Type, Type> proxyTypes = new Dictionary<Type, Type>();

        private static ModuleBuilder CreateProxyModule()
        {
            AssemblyName name = new AssemblyName("AutoMapper.Proxies");
            name.SetPublicKey(privateKey);
            name.SetPublicKeyToken(privateKeyToken);
            //AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            return builder.DefineDynamicModule("AutoMapper.Proxies.emit");
        }

        private static Type CreateProxyType(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("Only interfaces can be proxied", nameof(interfaceType));
            }
            string name =
                $"Proxy<{Regex.Replace(interfaceType.AssemblyQualifiedName ?? interfaceType.FullName ?? interfaceType.Name, @"[\s,]+", "_")}>";
            List<Type> allInterfaces = new List<Type>
            {
                interfaceType
            };
            allInterfaces.AddRange(interfaceType.GetInterfaces());
            Debug.WriteLine(name, "Emitting proxy type");
            TypeBuilder typeBuilder = proxyModule.DefineType(name,
                TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof (ProxyBase),
                allInterfaces.ToArray());
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes);
            ILGenerator ctorIl = constructorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, proxyBase_ctor);
            ctorIl.Emit(OpCodes.Ret);
            FieldBuilder propertyChangedField = null;
            if (typeof (INotifyPropertyChanged).IsAssignableFrom(interfaceType))
            {
                propertyChangedField = typeBuilder.DefineField("PropertyChanged", typeof (PropertyChangedEventHandler),
                    FieldAttributes.Private);
                MethodBuilder addPropertyChangedMethod = typeBuilder.DefineMethod("add_PropertyChanged",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual, typeof (void),
                    new[] {typeof (PropertyChangedEventHandler)});
                ILGenerator addIl = addPropertyChangedMethod.GetILGenerator();
                addIl.Emit(OpCodes.Ldarg_0);
                addIl.Emit(OpCodes.Dup);
                addIl.Emit(OpCodes.Ldfld, propertyChangedField);
                addIl.Emit(OpCodes.Ldarg_1);
                addIl.Emit(OpCodes.Call, delegate_Combine);
                addIl.Emit(OpCodes.Castclass, typeof (PropertyChangedEventHandler));
                addIl.Emit(OpCodes.Stfld, propertyChangedField);
                addIl.Emit(OpCodes.Ret);
                MethodBuilder removePropertyChangedMethod = typeBuilder.DefineMethod("remove_PropertyChanged",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual, typeof (void),
                    new[] {typeof (PropertyChangedEventHandler)});
                ILGenerator removeIl = removePropertyChangedMethod.GetILGenerator();
                removeIl.Emit(OpCodes.Ldarg_0);
                removeIl.Emit(OpCodes.Dup);
                removeIl.Emit(OpCodes.Ldfld, propertyChangedField);
                removeIl.Emit(OpCodes.Ldarg_1);
                removeIl.Emit(OpCodes.Call, delegate_Remove);
                removeIl.Emit(OpCodes.Castclass, typeof (PropertyChangedEventHandler));
                removeIl.Emit(OpCodes.Stfld, propertyChangedField);
                removeIl.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(addPropertyChangedMethod,
                    iNotifyPropertyChanged_PropertyChanged.GetAddMethod());
                typeBuilder.DefineMethodOverride(removePropertyChangedMethod,
                    iNotifyPropertyChanged_PropertyChanged.GetRemoveMethod());
            }
            List<PropertyInfo> propertiesToImplement = new List<PropertyInfo>();
            // first we collect all properties, those with setters before getters in order to enable less specific redundant getters
            foreach (
                PropertyInfo property in
                    allInterfaces.Where(intf => intf != typeof (INotifyPropertyChanged))
                        .SelectMany(intf => intf.GetProperties()))
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
            Dictionary<string, PropertyEmitter> fieldBuilders = new Dictionary<string, PropertyEmitter>();
            foreach (PropertyInfo property in propertiesToImplement)
            {
                PropertyEmitter propertyEmitter;
                if (fieldBuilders.TryGetValue(property.Name, out propertyEmitter))
                {
                    if ((propertyEmitter.PropertyType != property.PropertyType) &&
                        ((property.CanWrite) || (!property.PropertyType.IsAssignableFrom(propertyEmitter.PropertyType))))
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
                            new PropertyEmitter(typeBuilder, property.Name, property.PropertyType, propertyChangedField));
                }
                if (property.CanRead)
                {
                    typeBuilder.DefineMethodOverride(propertyEmitter.GetGetter(property.PropertyType),
                        property.GetGetMethod());
                }
                if (property.CanWrite)
                {
                    typeBuilder.DefineMethodOverride(propertyEmitter.GetSetter(property.PropertyType),
                        property.GetSetMethod());
                }
            }
            return typeBuilder.CreateType();
        }

        public Type GetProxyType(Type interfaceType)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }
            lock (proxyTypes)
            {
                Type proxyType;
                if (!proxyTypes.TryGetValue(interfaceType, out proxyType))
                {
                    proxyTypes.Add(interfaceType, proxyType = CreateProxyType(interfaceType));
                }
                return proxyType;
            }
        }

        private static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars/2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}

#endif