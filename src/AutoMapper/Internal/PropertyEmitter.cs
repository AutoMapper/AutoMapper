using System;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper.Internal {
	internal class PropertyEmitter {
		private static readonly MethodInfo proxyBase_NotifyPropertyChanged = typeof(ProxyBase).GetMethod("NotifyPropertyChanged", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Public, null, new[] {typeof(PropertyChangedEventHandler), typeof(string)}, null);

		private readonly FieldBuilder fieldBuilder;
		private readonly MethodBuilder getterBuilder;
		private readonly TypeBuilder owner;
		private readonly PropertyBuilder propertyBuilder;
		private readonly FieldBuilder propertyChangedField;
		private readonly MethodBuilder setterBuilder;

		public PropertyEmitter(TypeBuilder owner, string name, Type propertyType, FieldBuilder propertyChangedField) {
			this.owner = owner;
			this.propertyChangedField = propertyChangedField;
			fieldBuilder = owner.DefineField(String.Format("<{0}>", name), propertyType, FieldAttributes.Private);
			getterBuilder = owner.DefineMethod(String.Format("get_{0}", name), MethodAttributes.Public|MethodAttributes.Virtual|MethodAttributes.HideBySig|MethodAttributes.SpecialName, propertyType, Type.EmptyTypes);
			ILGenerator getterIl = getterBuilder.GetILGenerator();
			getterIl.Emit(OpCodes.Ldarg_0);
			getterIl.Emit(OpCodes.Ldfld, fieldBuilder);
			getterIl.Emit(OpCodes.Ret);
			setterBuilder = owner.DefineMethod(String.Format("set_{0}", name), MethodAttributes.Public|MethodAttributes.Virtual|MethodAttributes.HideBySig|MethodAttributes.SpecialName, typeof(void), new[] {propertyType});
			ILGenerator setterIl = setterBuilder.GetILGenerator();
			setterIl.Emit(OpCodes.Ldarg_0);
			setterIl.Emit(OpCodes.Ldarg_1);
			setterIl.Emit(OpCodes.Stfld, fieldBuilder);
			if (propertyChangedField != null) {
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

		public Type PropertyType {
			get {
				return propertyBuilder.PropertyType;
			}
		}

		public MethodBuilder GetGetter(Type requiredType) {
			if (!requiredType.IsAssignableFrom(PropertyType)) {
				throw new InvalidOperationException("Types are not compatible");
			}
			return getterBuilder;
		}

		public MethodBuilder GetSetter(Type requiredType) {
			if (!PropertyType.IsAssignableFrom(requiredType)) {
				throw new InvalidOperationException("Types are not compatible");
			}
			return setterBuilder;
		}
	}
}
