using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper.Internal
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ReflectionHelper
    {
        public static TypeMap[] GetIncludedTypeMaps(this IGlobalConfiguration configuration, TypeMap typeMap) => 
            configuration.GetIncludedTypeMaps(typeMap.IncludedDerivedTypes);
        public static MethodInfo GetConversionOperator(this TypePair context, string name)
        {
            foreach (MethodInfo sourceMethod in context.SourceType.GetMember(name, MemberTypes.Method, TypeExtensions.StaticFlags))
            {
                if (sourceMethod.ReturnType == context.DestinationType)
                {
                    return sourceMethod;
                }
            }
            return context.DestinationType.GetMethod(name, TypeExtensions.StaticFlags, null, new[] { context.SourceType }, null);
        }
        public static bool IsPublic(this PropertyInfo propertyInfo) => propertyInfo.GetGetMethod(true)?.IsPublic ?? propertyInfo.GetSetMethod(true).IsPublic;
        public static bool HasAnInaccessibleSetter(this PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }
        public static Type CreateType(this TypeBuilder type) => type.CreateTypeInfo().AsType();
        public static bool Has<TAttribute>(this MemberInfo member) where TAttribute : Attribute => member.IsDefined(typeof(TAttribute));
        public static bool CanBeSet(this MemberInfo propertyOrField) => propertyOrField is PropertyInfo property ? property.CanWrite : !((FieldInfo)propertyOrField).IsInitOnly;
        public static Expression GetDefaultValue(this ParameterInfo parameter)
        {
            if (parameter.DefaultValue == null && parameter.ParameterType.IsValueType)
            {
                return Default(parameter.ParameterType);
            }
            return Constant(parameter.DefaultValue);
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
        private static ArgumentOutOfRangeException Expected(MemberInfo propertyOrField) => new ArgumentOutOfRangeException(nameof(propertyOrField), "Expected a property or field, not " + propertyOrField);
        public static object GetMemberValue(this MemberInfo propertyOrField, object target) => propertyOrField switch
        {
            PropertyInfo property => property.GetValue(target, null),
            FieldInfo field => field.GetValue(target),
            _ => throw Expected(propertyOrField)
        };
        public static MemberInfo[] GetMemberPath(Type type, string fullMemberName)
        {
            var memberNames = fullMemberName.Split('.');
            var members = new MemberInfo[memberNames.Length];
            Type previousType = type;
            for(int index = 0; index < memberNames.Length; index++)
            {
                var currentType = GetCurrentType(previousType);
                var memberName = memberNames[index];
                var property = currentType.GetInheritedProperty(memberName);
                if (property != null)
                {
                    previousType = property.PropertyType;
                    members[index] = property;
                }
                else if (currentType.GetInheritedField(memberName) is FieldInfo field)
                {
                    previousType = field.FieldType;
                    members[index] = field;
                }
                else
                {
                    var method = currentType.GetInheritedMethod(memberName);
                    previousType = method.ReturnType;
                    members[index] = method;
                }
            }
            return members;
            static Type GetCurrentType(Type type) => type.IsGenericType && type.IsEnumerableType() ? type.GenericTypeArguments[0] : type;
        }
        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expressionToCheck = lambdaExpression.Body;
            var done = false;
            while (!done)
            {
                switch (expressionToCheck)
                {
                    case MemberExpression { Member: var member, Expression: { NodeType: ExpressionType.Parameter or ExpressionType.Convert } }:
                        return member;
                    case UnaryExpression { Operand: var operand }:
                        expressionToCheck = operand;
                        break;
                    default:
                        done = true;
                        break;
                }
            }
            throw new ArgumentException(
                $"Expression '{lambdaExpression}' must resolve to top-level member and not any child object's properties. You can use ForPath, a custom resolver on the child type or the AfterMap option instead.",
                nameof(lambdaExpression));
        }
        public static Type GetMemberType(this MemberInfo member) => member switch
        {
            PropertyInfo property => property.PropertyType,
            MethodInfo method => method.ReturnType,
            FieldInfo field => field.FieldType,
            null => throw new ArgumentNullException(nameof(member)),
            _ => throw new ArgumentOutOfRangeException(nameof(member))
        };
    }
}