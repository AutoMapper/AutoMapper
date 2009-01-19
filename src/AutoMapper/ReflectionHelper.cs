using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
	internal static class ReflectionHelper
	{
		public static MethodInfo FindModelMethodByName(MethodInfo[] getMethods, string nameToSearch)
		{
			string getName = "Get" + nameToSearch;
			return getMethods.FirstOrDefault(m => (String.Compare(m.Name, getName, StringComparison.Ordinal) == 0) || (String.Compare(m.Name, nameToSearch, StringComparison.Ordinal) == 0));
		}

		public static PropertyInfo FindModelPropertyByName(PropertyInfo[] modelProperties, string nameToSearch)
		{
			return modelProperties.FirstOrDefault(prop => String.Compare(prop.Name, nameToSearch, StringComparison.Ordinal) == 0);
		}

		public static PropertyInfo FindProperty(LambdaExpression lambdaExpression)
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
						var propertyInfo = ((MemberExpression) expressionToCheck).Member as PropertyInfo;
						return propertyInfo;
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

			public static PropertyInfo[] GetPublicGetProperties(this Type type)
			{
				return type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance,
				                        (m, f) => ((PropertyInfo) m).CanRead, null)
					.Cast<PropertyInfo>()
					.ToArray();
			}
		}
	}
}