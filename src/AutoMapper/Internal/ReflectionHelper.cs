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
						expressionToCheck = ((LambdaExpression)expressionToCheck).Body;
						break;
					case ExpressionType.MemberAccess:
						var memberExpression = ((MemberExpression)expressionToCheck);

						if (memberExpression.Expression.NodeType != ExpressionType.Parameter && 
                            memberExpression.Expression.NodeType != ExpressionType.Convert)
						{
							throw new ArgumentException(string.Format("Expression '{0}' must resolve to top-level member.", lambdaExpression), "lambdaExpression");
						}

						MemberInfo member = memberExpression.Member;
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
            private static Dictionary<Type, IMemberAccessor[]> _publicReadAccessorsCache = new Dictionary<Type, IMemberAccessor[]>();
            private static Dictionary<Type, MethodInfo[]> _publicNoArgMethodsCache = new Dictionary<Type, MethodInfo[]>();
            private static object _accessorSync = new object();
            private static object _methodSync = new object();

			public static MethodInfo[] GetPublicNoArgMethods(this Type type)
			{
			    MethodInfo[] methods;

                if (!_publicNoArgMethodsCache.TryGetValue(type, out methods))
                {
                    lock(_methodSync)
                    {
                        if (!_publicNoArgMethodsCache.TryGetValue(type, out methods))
                        {
                            methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(m => (m.ReturnType != null) && (m.GetParameters().Length == 0) && (m.MemberType == MemberTypes.Method))
                                .ToArray();
                            _publicNoArgMethodsCache[type] = methods;
                        }
                    }
                }

                return methods;
			}

			public static IMemberAccessor[] GetPublicReadAccessors(this Type type)
			{
			    IMemberAccessor[] members;

                if (!_publicReadAccessorsCache.TryGetValue(type, out members))
                {
                    lock (_methodSync)
                    {
                        if (!_publicReadAccessorsCache.TryGetValue(type, out members))
                        {
                            // Collect that target type, its base type, and all implemented/inherited interface types
                            IEnumerable<Type> typesToScan = new[] { type, type.BaseType };

                            if (type.IsInterface)
                                typesToScan = typesToScan.Concat(type.FindInterfaces((m, f) => true, null));

                            // Scan all types for public properties and fields
                            var allMembers = typesToScan
                                .Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
                                .SelectMany(x => x.FindMembers(MemberTypes.Property | MemberTypes.Field,
                                                                 BindingFlags.Instance | BindingFlags.Public,
                                                                 (m, f) =>
                                                                 m is FieldInfo ||
                                                                 (m is PropertyInfo && ((PropertyInfo)m).CanRead && !((PropertyInfo)m).GetIndexParameters().Any()), null));

                            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
                            var filteredMembers = allMembers
                                .OfType<PropertyInfo>()
                                .GroupBy(x => x.Name) // group properties of the same name together
                                .Select(x =>
                                    x.Any(y => y.CanRead && y.CanWrite) ? // favor the first property that can both read & write - otherwise pick the first one
                                        x.Where(y => y.CanRead && y.CanWrite).First() :
                                        x.First())
                                .OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                                .Concat(allMembers.Where(x => x is FieldInfo));  // add FieldInfo objects back

                            members = filteredMembers
                                .Select(x => x.ToMemberAccessor())
                                .ToArray();

                            _publicReadAccessorsCache[type] = members;
                        }
                    }
                }

                return members;
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
