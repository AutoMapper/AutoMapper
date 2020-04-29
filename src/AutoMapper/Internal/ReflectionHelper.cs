using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Internal
{
    using Configuration;

    public static class ReflectionHelper
    {
        public static bool CanBeSet(MemberInfo propertyOrField)
        {
            return propertyOrField is FieldInfo field ? 
                        !field.IsInitOnly : 
                        ((PropertyInfo)propertyOrField).CanWrite;
        }

        public static object GetDefaultValue(ParameterInfo parameter)
        {
            if (parameter.DefaultValue == null && parameter.ParameterType.IsValueType)
            {
                return Activator.CreateInstance(parameter.ParameterType);
            }
            return parameter.DefaultValue;
        }

        public static object MapMember(ResolutionContext context, MemberInfo member, object value, object destination = null)
        {
            var memberType = GetMemberType(member);
            var destValue = destination == null ? null : GetMemberValue(member, destination);
            return context.Map(value, destValue, value?.GetType() ?? typeof(object), memberType, DefaultMemberMap.Instance);
        }

        public static bool IsDynamic(object obj) => obj is IDynamicMetaObjectProvider;

        public static bool IsDynamic(Type type) => typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);

        public static void SetMemberValue(MemberInfo propertyOrField, object target, object value)
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

        public static object GetMemberValue(MemberInfo propertyOrField, object target)
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

        public static IEnumerable<MemberInfo> GetMemberPath(Type type, string fullMemberName)
        {
            MemberInfo property = null;
            foreach (var memberName in fullMemberName.Split('.'))
            {
                var currentType = GetCurrentType(property, type);
                yield return property = currentType.GetFieldOrProperty(memberName);
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

        public static MemberInfo GetFieldOrProperty(LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            return memberExpression != null
                ? memberExpression.Member
                : throw new ArgumentOutOfRangeException(nameof(expression), "Expected a property/field access expression, not " + expression);
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

        public static Type GetMemberType(MemberInfo memberInfo)
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

        /// <summary>
        /// if targetType is oldType, method will return newType
        /// if targetType is not oldType, method will return targetType
        /// if targetType is generic type with oldType arguments, method will replace all oldType arguments on newType
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="oldType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public static Type ReplaceItemType(Type targetType, Type oldType, Type newType)
        {
            if (targetType == oldType)
                return newType;

            if (targetType.IsGenericType)
            {
                var genSubArgs = targetType.GetTypeInfo().GenericTypeArguments;
                var newGenSubArgs = new Type[genSubArgs.Length];
                for (var i = 0; i < genSubArgs.Length; i++)
                    newGenSubArgs[i] = ReplaceItemType(genSubArgs[i], oldType, newType);
                return targetType.GetGenericTypeDefinition().MakeGenericType(newGenSubArgs);
            }

            return targetType;
        }
    }
}
