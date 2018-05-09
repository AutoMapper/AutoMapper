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

        public static object MapMember(this ResolutionContext context, MemberInfo member, object value, object destination = null)
            => ReflectionHelper.MapMember(context, member, value, destination);

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

        public static IEnumerable<TypeInfo> GetDefinedTypes(this Assembly assembly) =>
            assembly.DefinedTypes;

        public static bool GetHasDefaultValue(this ParameterInfo info) =>
            info.HasDefaultValue;

        public static bool GetIsConstructedGenericType(this Type type) =>
            type.IsConstructedGenericType;
    }
}
