#if NET45
namespace AutoMapper.Execution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public class PropertyEmitter
    {
        private static readonly MethodInfo proxyBase_NotifyPropertyChanged =
            typeof (ProxyBase).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "NotifyPropertyChanged");

        private readonly FieldBuilder fieldBuilder;
        private readonly MethodBuilder getterBuilder;
        private readonly PropertyBuilder propertyBuilder;
        private readonly MethodBuilder setterBuilder;

        public PropertyEmitter(TypeBuilder owner, string name, Type propertyType, FieldBuilder propertyChangedField)
        {
            fieldBuilder = owner.DefineField($"<{name}>", propertyType, FieldAttributes.Private);
            getterBuilder = owner.DefineMethod($"get_{name}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName, propertyType, new Type[0]);
            ILGenerator getterIl = getterBuilder.GetILGenerator();
            getterIl.Emit(OpCodes.Ldarg_0);
            getterIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIl.Emit(OpCodes.Ret);
            setterBuilder = owner.DefineMethod($"set_{name}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName, typeof (void), new[] {propertyType});
            ILGenerator setterIl = setterBuilder.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0);
            setterIl.Emit(OpCodes.Ldarg_1);
            setterIl.Emit(OpCodes.Stfld, fieldBuilder);
            if (propertyChangedField != null)
            {
                setterIl.Emit(OpCodes.Ldarg_0);
                setterIl.Emit(OpCodes.Dup);
                setterIl.Emit(OpCodes.Ldfld, propertyChangedField);
                setterIl.Emit(OpCodes.Ldstr, name);
                setterIl.Emit(OpCodes.Call, proxyBase_NotifyPropertyChanged);
            }
            setterIl.Emit(OpCodes.Ret);
            propertyBuilder = owner.DefineProperty(name, PropertyAttributes.None, propertyType, null);
            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
        }

        public Type PropertyType => propertyBuilder.PropertyType;

        public MethodBuilder GetGetter(Type requiredType)
        {
            if (!requiredType.IsAssignableFrom(PropertyType))
            {
                throw new InvalidOperationException("Types are not compatible");
            }
            return getterBuilder;
        }

        public MethodBuilder GetSetter(Type requiredType)
        {
            if (!PropertyType.IsAssignableFrom(requiredType))
            {
                throw new InvalidOperationException("Types are not compatible");
            }
            return setterBuilder;
        }
    }
}
#endif