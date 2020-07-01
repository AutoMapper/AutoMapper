using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ReflectionHelper
    {
        public static bool IsStatic(this FieldInfo fieldInfo) => fieldInfo?.IsStatic ?? false;

        public static bool IsStatic(this PropertyInfo propertyInfo) => propertyInfo?.GetGetMethod(true)?.IsStatic
                                                                       ?? propertyInfo?.GetSetMethod(true)?.IsStatic
                                                                       ?? false;

        public static bool IsStatic(this MemberInfo memberInfo) => (memberInfo as FieldInfo).IsStatic()
                                                                   || (memberInfo as PropertyInfo).IsStatic()
                                                                   || ((memberInfo as MethodInfo)?.IsStatic
                                                                       ?? false);

        public static bool IsPublic(this PropertyInfo propertyInfo) => (propertyInfo?.GetGetMethod(true)?.IsPublic ?? false)
                                                                       || (propertyInfo?.GetSetMethod(true)?.IsPublic ?? false);

        public static bool HasAnInaccessibleSetter(this PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }

        public static bool IsPublic(this MemberInfo memberInfo) => (memberInfo as FieldInfo)?.IsPublic ?? (memberInfo as PropertyInfo).IsPublic();

        public static Type CreateType(this TypeBuilder type) => type.CreateTypeInfo().AsType();

        public static bool Has<TAttribute>(this MemberInfo member) where TAttribute : Attribute => member.GetCustomAttribute<TAttribute>() != null;

        public static bool CanBeSet(this MemberInfo propertyOrField) => propertyOrField is FieldInfo field ? !field.IsInitOnly : ((PropertyInfo)propertyOrField).CanWrite;

        public static object GetDefaultValue(this ParameterInfo parameter)
        {
            if (parameter.DefaultValue == null && parameter.ParameterType.IsValueType)
            {
                return Activator.CreateInstance(parameter.ParameterType);
            }
            return parameter.DefaultValue;
        }

        public static object MapMember(this ResolutionContext context, MemberInfo member, object value, object destination = null)
        {
            var memberType = GetMemberType(member);
            var destValue = destination == null ? null : GetMemberValue(member, destination);
            return context.Map(value, destValue, value?.GetType() ?? typeof(object), memberType, DefaultMemberMap.Instance);
        }

        public static void SetMemberValue(this MemberInfo propertyOrField, object target, object value)
        {
            if (propertyOrField is PropertyInfo property)
            {
                property.SetValue(target, value, null);
                return;
            }
            if (propertyOrField is FieldInfo field)
            {
                field.SetValue(target, value);
                return;
            }
            throw Expected(propertyOrField);
        }

        private static ArgumentOutOfRangeException Expected(MemberInfo propertyOrField)
            => new ArgumentOutOfRangeException(nameof(propertyOrField), "Expected a property or field, not " + propertyOrField);

        public static object GetMemberValue(this MemberInfo propertyOrField, object target)
        {
            if (propertyOrField is PropertyInfo property)
            {
                return property.GetValue(target, null);
            }
            if (propertyOrField is FieldInfo field)
            {
                return field.GetValue(target);
            }
            throw Expected(propertyOrField);
        }

        public static MemberInfo[] GetMemberPath(Type type, string fullMemberName)
        {
            return GetMemberPathCore().ToArray();
            IEnumerable<MemberInfo> GetMemberPathCore()
            {
                MemberInfo property = null;
                foreach (var memberName in fullMemberName.Split('.'))
                {
                    var currentType = GetCurrentType(property, type);
                    yield return property = currentType.GetFieldOrProperty(memberName);
                }
            }
        }

        private static Type GetCurrentType(MemberInfo member, Type type)
        {
            var memberType = member?.GetMemberType() ?? type;
            if (memberType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(memberType))
            {
                memberType = memberType.GetTypeInfo().GenericTypeArguments[0];
            }
            return memberType;
        }

        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expressionToCheck = lambdaExpression;

            var done = false;

            while (!done)
            {
                switch (expressionToCheck.NodeType)
                {
                    case ExpressionType.Convert:
                        expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
                        break;
                    case ExpressionType.Lambda:
                        expressionToCheck = ((LambdaExpression)expressionToCheck).Body;
                        break;
                    case ExpressionType.MemberAccess:
                        var memberExpression = ((MemberExpression)expressionToCheck);

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter &&
                            memberExpression.Expression.NodeType != ExpressionType.Convert)
                        {
                            throw new ArgumentException(
                                $"Expression '{lambdaExpression}' must resolve to top-level member and not any child object's properties. You can use ForPath, a custom resolver on the child type or the AfterMap option instead.",
                                nameof(lambdaExpression));
                        }

                        var member = memberExpression.Member;

                        return member;
                    default:
                        done = true;
                        break;
                }
            }

            throw new AutoMapperConfigurationException(
                "Custom configuration for members is only supported for top-level individual members on a type.");
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case MethodInfo mInfo:
                    return mInfo.ReturnType;
                case PropertyInfo pInfo:
                    return pInfo.PropertyType;
                case FieldInfo fInfo:
                    return fInfo.FieldType;
                case null:
                    throw new ArgumentNullException(nameof(memberInfo));
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberInfo));
            }
        }
    }
}