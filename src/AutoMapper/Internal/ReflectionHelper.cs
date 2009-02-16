using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
	internal static class ReflectionHelper
	{
		public static MethodInfo FindModelMethodByName(MethodInfo[] getMethods, string nameToSearch)
		{
			string getName = "Get" + nameToSearch;
			return getMethods.FirstOrDefault(m => (String.Compare(m.Name, getName, StringComparison.Ordinal) == 0) || (String.Compare(m.Name, nameToSearch, StringComparison.Ordinal) == 0));
		}

		public static IMemberAccessor FindModelPropertyByName(IMemberAccessor[] modelProperties, string nameToSearch)
		{
			return modelProperties.FirstOrDefault(prop => String.Compare(prop.Name, nameToSearch, StringComparison.Ordinal) == 0);
		}

		public static IMemberAccessor FindProperty(LambdaExpression lambdaExpression)
		{
			Expression expressionToCheck = lambdaExpression;

			bool done = false;

			while (!done)
			{
				switch (expressionToCheck.NodeType)
				{
					case ExpressionType.Convert:
						expressionToCheck = ((UnaryExpression) expressionToCheck).Operand;
						break;
					case ExpressionType.Lambda:
						expressionToCheck = lambdaExpression.Body;
						break;
					case ExpressionType.MemberAccess:
						MemberInfo member = ((MemberExpression) expressionToCheck).Member;
                        if (member is FieldInfo) return new FieldAccessor((FieldInfo) member);
                        if (member is PropertyInfo) return new PropertyAccessor((PropertyInfo) member);
                        return null;
					default:
						done = true;
						break;
				}
			}

			return null;
		}
	}

	namespace ReflectionExtensions
	{
		internal static class ReflectionHelper
		{
			public static MethodInfo[] GetPublicNoArgMethods(this Type type)
			{
				return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
					.Where(m => (m.ReturnType != null) && (m.GetParameters().Length == 0) && (m.MemberType == MemberTypes.Method))
					.ToArray();
			}

            public static IMemberAccessor[] GetPublicReadAccessors(this Type type)
            {
                MemberInfo[] members = type.FindMembers(MemberTypes.Property | MemberTypes.Field,
                                                         BindingFlags.Instance | BindingFlags.Public,
                                                         (m, f) =>
                                                         m is FieldInfo ||
                                                         (m is PropertyInfo && ((PropertyInfo) m).CanRead),null);
                
                var accessors = new IMemberAccessor[members.Length];
                for (int i = 0; i < members.Length; i++)
                    accessors[i] = members[i].ToMemberAccessor();
                return accessors;

            }

            public static IMemberAccessor GetAccessor(this Type targetType,string accessorName,BindingFlags bindingFlags)
            {
                MemberInfo[] members = targetType.GetMember(accessorName, bindingFlags);
                return
                    members.FirstOrDefault(member => member is PropertyInfo || member is FieldInfo)
                    .ToMemberAccessor();
            }

            public static IMemberAccessor ToMemberAccessor(this MemberInfo accessorCandidate)
            {
                if (accessorCandidate == null)
                    return null;

                if (accessorCandidate is PropertyInfo)
                    return new PropertyAccessor((PropertyInfo) accessorCandidate);

                if (accessorCandidate is FieldInfo)
                    return new FieldAccessor((FieldInfo) accessorCandidate);

                return null;
            }
		}
	}
}
