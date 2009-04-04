using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using System.Collections.Generic;

namespace AutoMapper
{
	internal static class ReflectionHelper
	{
		public static MethodInfo FindModelMethodByName(MethodInfo[] getMethods, string nameToSearch)
		{
			string getName = "Get" + nameToSearch;
			return getMethods.FirstOrDefault(m => (NameMatches(m.Name, getName)) || NameMatches(m.Name, nameToSearch));
		}

		public static IMemberAccessor FindModelPropertyByName(IMemberAccessor[] modelProperties, string nameToSearch)
		{
			return modelProperties.FirstOrDefault(prop => NameMatches(prop.Name, nameToSearch));
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
						expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
						break;
					case ExpressionType.Lambda:
						expressionToCheck = lambdaExpression.Body;
						break;
					case ExpressionType.MemberAccess:
						MemberInfo member = ((MemberExpression)expressionToCheck).Member;
						if (member is FieldInfo) return new FieldAccessor((FieldInfo)member);
						if (member is PropertyInfo) return new PropertyAccessor((PropertyInfo)member);
						return null;
					default:
						done = true;
						break;
				}
			}

			return null;
		}

		private static bool NameMatches(string memberName, string nameToMatch)
		{
			return String.Compare(memberName, nameToMatch, StringComparison.OrdinalIgnoreCase) == 0;
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
				// Collect that target type, its base type, and all implemented/inherited interface types
				var typesToScan = new[] { type, type.BaseType }
					.Concat(type.FindInterfaces((m, f) => true, null));

				// Scan all types for public properties and fields
				var members = typesToScan
					.Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
					.SelectMany(x => x.FindMembers(MemberTypes.Property | MemberTypes.Field,
													 BindingFlags.Instance | BindingFlags.Public,
													 (m, f) =>
													 m is FieldInfo ||
													 (m is PropertyInfo && ((PropertyInfo)m).CanRead), null));

				// Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
				var filteredMembers = members
					.Where(x => x is PropertyInfo) // only interested in filtering properties
					.OfType<PropertyInfo>()
					.GroupBy(x => x.Name) // group properties of the same name together
					.Select(x =>
						x.Any(y => y.CanRead && y.CanWrite) ? // favor the first property that can both read & write - otherwise pick the first one
							x.Where(y => y.CanRead && y.CanWrite).First() :
							x.First())
					.OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
					.Concat(members.Where(x => x is FieldInfo));  // add FieldInfo objects back

				return filteredMembers
					.Select(x => x.ToMemberAccessor())
					.ToArray();
			}

			public static IMemberAccessor GetAccessor(this Type targetType, string accessorName, BindingFlags bindingFlags)
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
					return new PropertyAccessor((PropertyInfo)accessorCandidate);

				if (accessorCandidate is FieldInfo)
					return new FieldAccessor((FieldInfo)accessorCandidate);

				return null;
			}
		}
	}
}
