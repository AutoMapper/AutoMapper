using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    internal static class ReflectionExtensions
    {
        public static object GetDefaultValue(this ParameterInfo parameter)
            => ReflectionHelper.GetDefaultValue(parameter);

        public static object MapMember(this ResolutionContext context, MemberInfo member, object value, object destination)
            => ReflectionHelper.MapMember(context, member, value, destination);

        public static object MapMember(this ResolutionContext context, MemberInfo member, object value)
            => ReflectionHelper.MapMember(context, member, value);

        public static bool IsDynamic(this object obj)
            => ReflectionHelper.IsDynamic(obj);

        public static bool IsDynamic(this Type type)
            => ReflectionHelper.IsDynamic(type);

        public static void SetMemberValue(this MemberInfo propertyOrField, object target, object value)
            => ReflectionHelper.SetMemberValue(propertyOrField, target, value);

        public static object GetMemberValue(this MemberInfo propertyOrField, object target)
            => ReflectionHelper.GetMemberValue(propertyOrField, target);

        public static IEnumerable<MemberInfo> GetMemberPath(Type type, string fullMemberName)
            => ReflectionHelper.GetMemberPath(type, fullMemberName);

        public static MemberInfo GetFieldOrProperty(this LambdaExpression expression)
            => ReflectionHelper.GetFieldOrProperty(expression);

        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
            => ReflectionHelper.FindProperty(lambdaExpression);

        public static Type GetMemberType(this MemberInfo memberInfo)
            => ReflectionHelper.GetMemberType(memberInfo);

        /// <summary>
        /// if targetType is oldType, method will return newType
        /// if targetType is not oldType, method will return targetType
        /// if targetType is generic type with oldType arguments, method will replace all oldType arguments on newType
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="oldType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public static Type ReplaceItemType(this Type targetType, Type oldType, Type newType)
            => ReflectionHelper.ReplaceItemType(targetType, oldType, newType);

#if NET40
        public static TypeInfo GetTypeInfo(this Type type)
        {
            return TypeInfo.FromType(type);
        }

        public static IEnumerable<TypeInfo> GetDefinedTypes(this Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            TypeInfo[] array = new TypeInfo[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                TypeInfo typeInfo = types[i].GetTypeInfo();
                array[i] = typeInfo ?? throw new NotSupportedException();
            }
            return array;
        }

        public static bool GetHasDefaultValue(this ParameterInfo info) =>
            info.GetDefaultValue() != DBNull.Value;
        
        public static bool GetIsConstructedGenericType(this Type type) =>
            type.IsGenericType && !type.IsGenericTypeDefinition;
#else
        public static IEnumerable<TypeInfo> GetDefinedTypes(this Assembly assembly) =>
            assembly.DefinedTypes;

        public static bool GetHasDefaultValue(this ParameterInfo info) =>
            info.HasDefaultValue;

        public static bool GetIsConstructedGenericType(this Type type) =>
            type.IsConstructedGenericType;
#endif
    }
}

#if NET40
namespace System.Reflection
{
    using System.Collections.Concurrent;
    using System.Globalization;

    [Serializable]
    internal class TypeInfo : Type
    {
        public static readonly ConcurrentDictionary<Type, TypeInfo> _typeInfoCache = new ConcurrentDictionary<Type, TypeInfo>();

        public static TypeInfo FromType(Type type)
        {
            return _typeInfoCache.GetOrAdd(type, t => new TypeInfo(t));
        }

        public IEnumerable<Type> ImplementedInterfaces => GetInterfaces();
        public Type[] GenericTypeParameters
        {
            get
            {
                if (IsGenericTypeDefinition)
                    return GetGenericArguments();
                return EmptyTypes;
            }
        }

        public Type[] GenericTypeArguments
        {
            get
            {
                if (IsGenericType && !IsGenericTypeDefinition)
                    return GetGenericArguments();
                return EmptyTypes;
            }
        }

        public virtual IEnumerable<ConstructorInfo> DeclaredConstructors =>
            GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public virtual IEnumerable<MemberInfo> DeclaredMembers =>
            GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public virtual IEnumerable<MethodInfo> DeclaredMethods =>
            GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public virtual IEnumerable<PropertyInfo> DeclaredProperties =>
            GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public override Guid GUID => _type.GUID;

        public override Module Module => _type.Module;

        public override Assembly Assembly => _type.Assembly;

        public override string FullName => _type.FullName;

        public override string Namespace => _type.Namespace;

        public override string AssemblyQualifiedName => _type.AssemblyQualifiedName;

        public override Type BaseType => _type.BaseType;

        public override Type UnderlyingSystemType => _type.UnderlyingSystemType;

        public override string Name => _type.Name;

        private readonly Type _type;

        protected TypeInfo(Type type)
        {
            _type = type;
        }

        public Type AsType() => this;

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) =>
            _type.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) =>
            _type.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) =>
            _type.GetConstructors(bindingAttr);

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) =>
            _type.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr) =>
            _type.GetMethods(bindingAttr);

        public override FieldInfo GetField(string name, BindingFlags bindingAttr) =>
            _type.GetField(name, bindingAttr);

        public override FieldInfo[] GetFields(BindingFlags bindingAttr) =>
            _type.GetFields(bindingAttr);

        public override Type GetInterface(string name, bool ignoreCase) =>
            _type.GetInterface(name, ignoreCase);

        public override Type[] GetInterfaces() =>
            _type.GetInterfaces();

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr) =>
            _type.GetEvent(name, bindingAttr);

        public override EventInfo[] GetEvents(BindingFlags bindingAttr) =>
            _type.GetEvents(bindingAttr);

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) =>
            _type.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) =>
            _type.GetProperties(bindingAttr);

        public override Type[] GetNestedTypes(BindingFlags bindingAttr) =>
            _type.GetNestedTypes(bindingAttr);

        public override Type GetNestedType(string name, BindingFlags bindingAttr) =>
            _type.GetNestedType(name, bindingAttr);

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr) =>
            _type.GetMembers(bindingAttr);

        protected override TypeAttributes GetAttributeFlagsImpl() =>
            _type.Attributes;

        protected override bool IsArrayImpl() =>
            _type.IsArray;

        protected override bool IsByRefImpl() =>
            _type.IsByRef;

        protected override bool IsPointerImpl() =>
            _type.IsPointer;

        protected override bool IsPrimitiveImpl() =>
            _type.IsPrimitive;

        protected override bool IsCOMObjectImpl() =>
            _type.IsCOMObject;

        public override Type GetElementType() =>
            _type.GetElementType();

        protected override bool HasElementTypeImpl() =>
            _type.HasElementType;

        public override object[] GetCustomAttributes(bool inherit) =>
            _type.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) =>
            _type.GetCustomAttributes(attributeType, inherit);

        public override bool IsDefined(Type attributeType, bool inherit) =>
            _type.IsDefined(attributeType, inherit);
    }

    static class CustomAttributeExtensions
    {
        public static T GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(element, typeof(T), inherit);
        }
    }

    static class RuntimeReflectionExtensions
    {
        public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type) =>
            type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type) =>
             type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public static FieldInfo GetRuntimeField(this Type type, string name) =>
             type.GetField(name);
    }
}
#endif