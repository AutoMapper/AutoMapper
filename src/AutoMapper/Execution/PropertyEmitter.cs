namespace AutoMapper.Execution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public class PropertyEmitter
    {
        private static readonly MethodInfo ProxyBaseNotifyPropertyChanged =
            typeof (ProxyBase).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "NotifyPropertyChanged");

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
            if(!property.CanWrite)
            {
                return;
            }
            _setterBuilder = owner.DefineMethod($"set_{name}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName, typeof (void), new[] {propertyType});
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
