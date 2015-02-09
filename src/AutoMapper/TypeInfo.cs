using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Internal;

namespace AutoMapper
{
    using System.Diagnostics;

    /// <summary>
    /// Contains cached reflection information for easy retrieval
    /// </summary>
    [DebuggerDisplay("{Type}")]
    public class TypeInfo
    {
        private readonly MemberInfo[] _publicGetters;
        private readonly MemberInfo[] _publicAccessors;
        private readonly MethodInfo[] _publicGetMethods;
        private readonly ConstructorInfo[] _constructors;
        private readonly MethodInfo[] _extensionMethods;

        public Type Type { get; }

        public TypeInfo(Type type)
            : this (type, new MethodInfo[0])
        {
        }

        public TypeInfo(Type type, IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            Type = type;
        	var publicReadableMembers = GetAllPublicReadableMembers();
            var publicWritableMembers = GetAllPublicWritableMembers();
			_publicGetters = BuildPublicReadAccessors(publicReadableMembers);
            _publicAccessors = BuildPublicAccessors(publicWritableMembers);
            _publicGetMethods = BuildPublicNoArgMethods();
            _constructors = type.GetDeclaredConstructors().Where(ci => !ci.IsStatic).ToArray();
            _extensionMethods = BuildPublicNoArgExtensionMethods(sourceExtensionMethodSearch);
        }

        public IEnumerable<ConstructorInfo> GetConstructors()
        {
            return _constructors;
        }

        public IEnumerable<MemberInfo> GetPublicReadAccessors()
        {
            return _publicGetters;
        }

		public IEnumerable<MemberInfo> GetPublicWriteAccessors()
        {
            return _publicAccessors;
        }

        public IEnumerable<MethodInfo> GetPublicNoArgMethods()
        {
            return _publicGetMethods;
        }

		public IEnumerable<MethodInfo> GetPublicNoArgExtensionMethods()
		{
		    return _extensionMethods;
		}

        private MethodInfo[] BuildPublicNoArgExtensionMethods(IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            var sourceExtensionMethodSearchArray = sourceExtensionMethodSearch.ToArray();

            var explicitExtensionMethods = sourceExtensionMethodSearchArray
                .Where(method => method.GetParameters()[0].ParameterType == Type)
                .ToList();

            var genericInterfaces = Type.GetInterfaces().Where(t => t.IsGenericType()).ToList();

            if (Type.IsInterface() && Type.IsGenericType())
                genericInterfaces.Add(Type);

            foreach (var method in sourceExtensionMethodSearchArray
                .Where(method => method.IsGenericMethodDefinition))
            {
                var parameterType = method.GetParameters()[0].ParameterType;

                var matchingLength = genericInterfaces
                    .Where(t =>
                    {
                        var length = t.GetGenericParameters().Length;
                        var otherLength = parameterType.GetGenericArguments().Length;
                        return length ==
                               otherLength;
                    }).ToArray();
                var interfaceMatch = matchingLength
                    .Where(t => method.MakeGenericMethod(t.GetGenericArguments()).GetParameters()[0].ParameterType.IsAssignableFrom(t))
                    .FirstOrDefault();

                if (interfaceMatch != null)
                {
                    explicitExtensionMethods.Add(method.MakeGenericMethod(interfaceMatch.GetGenericArguments()));
                }
            }

            return explicitExtensionMethods.ToArray();
        }

        private MemberInfo[] BuildPublicReadAccessors(IEnumerable<MemberInfo> allMembers)
        {
			// Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var filteredMembers = allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
                .OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                .Concat(allMembers.Where(x => x is FieldInfo));  // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

        private MemberInfo[] BuildPublicAccessors(IEnumerable<MemberInfo> allMembers)
        {
        	// Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var filteredMembers = allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x =>
                    x.Any(y => y.CanWrite && y.CanRead) ? // favor the first property that can both read & write - otherwise pick the first one
						x.First(y => y.CanWrite && y.CanRead) :
                        x.First())
				.Where(pi => pi.CanWrite || pi.PropertyType.IsListOrDictionaryType())
                .OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                .Concat(allMembers.Where(x => x is FieldInfo));  // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

    	private IEnumerable<MemberInfo> GetAllPublicReadableMembers()
    	{
            return GetAllPublicMembers(PropertyReadable, mi => !mi.IsStatic() && mi.IsPublic());
    	}

        private IEnumerable<MemberInfo> GetAllPublicWritableMembers()
        {
            return GetAllPublicMembers(PropertyWritable, mi => !mi.IsStatic() && mi.IsPublic());
        }

        private bool PropertyReadable(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead;
        }

        private bool PropertyWritable(PropertyInfo propertyInfo)
        {
            bool propertyIsEnumerable = (typeof(string) != propertyInfo.PropertyType)
                                         && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);

            return propertyInfo.CanWrite || propertyIsEnumerable;
        }

        private IEnumerable<MemberInfo> GetAllPublicMembers(Func<PropertyInfo, bool> propertyAvailableFor, Func<MemberInfo, bool> memberAvailableFor)
        {
            var typesToScan = new List<Type>();
            for (var t = Type; t != null; t = t.BaseType())
                typesToScan.Add(t);

            if (Type.IsInterface())
                typesToScan.AddRange(Type.GetInterfaces());

            // Scan all types for public properties and fields
            return typesToScan
                .Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
                .SelectMany(x => x.GetDeclaredMembers()
                        .Where(mi => mi.DeclaringType != null && mi.DeclaringType == x)
                        .Where(memberAvailableFor)
                        .Where(
                            m =>
                                m is FieldInfo ||
                                (m is PropertyInfo && propertyAvailableFor((PropertyInfo) m) &&
                                 !((PropertyInfo) m).GetIndexParameters().Any()))
                );
        }

        private MethodInfo[] BuildPublicNoArgMethods()
        {
            return Type.GetDeclaredMethods()
                .Where(mi => mi.IsPublic && !mi.IsStatic)
                .Where(m => (m.ReturnType != typeof(void)) && (m.GetParameters().Length == 0))
                .ToArray();
        }
    }
}
