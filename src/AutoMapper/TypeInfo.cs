namespace AutoMapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// Contains cached reflection information for easy retrieval
    /// </summary>
    [DebuggerDisplay("{Type}")]
    public class TypeInfo
    {
        private readonly MethodInfo[] _publicGetMethods;
        private readonly MethodInfo[] _extensionMethods;

        public Type Type { get; }

        public TypeInfo(Type type)
            : this(type, new MethodInfo[0])
        {
        }

        public TypeInfo(Type type, IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            Type = type;
            var publicReadableMembers = GetAllPublicReadableMembers();
            var publicWritableMembers = GetAllPublicWritableMembers();
            PublicReadAccessors = BuildPublicReadAccessors(publicReadableMembers);
            PublicWriteAccessors = BuildPublicAccessors(publicWritableMembers);
            PublicNoArgMethods = BuildPublicNoArgMethods();
            Constructors = type.GetDeclaredConstructors().Where(ci => !ci.IsStatic).ToArray();
            PublicNoArgExtensionMethods = BuildPublicNoArgExtensionMethods(sourceExtensionMethodSearch);
        }

        public IEnumerable<ConstructorInfo> Constructors { get; }

        public IEnumerable<MemberInfo> PublicReadAccessors { get; }

        public IEnumerable<MemberInfo> PublicWriteAccessors { get; }

        public IEnumerable<MethodInfo> PublicNoArgMethods { get; }

        public IEnumerable<MethodInfo> PublicNoArgExtensionMethods { get; }

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
                    .FirstOrDefault(t => method.MakeGenericMethod(t.GetGenericArguments()).GetParameters()[0].ParameterType
                                .IsAssignableFrom(t));

                if (interfaceMatch != null)
                {
                    explicitExtensionMethods.Add(method.MakeGenericMethod(interfaceMatch.GetGenericArguments()));
                }
            }

            return explicitExtensionMethods.ToArray();
        }

        private static MemberInfo[] BuildPublicReadAccessors(IEnumerable<MemberInfo> allMembers)
        {
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var filteredMembers = allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
                .OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                .Concat(allMembers.Where(x => x is FieldInfo)); // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

        private static MemberInfo[] BuildPublicAccessors(IEnumerable<MemberInfo> allMembers)
        {
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var filteredMembers = allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x =>
                    x.Any(y => y.CanWrite && y.CanRead)
                        ? // favor the first property that can both read & write - otherwise pick the first one
                        x.First(y => y.CanWrite && y.CanRead)
                        : x.First())
                .Where(pi => pi.CanWrite || pi.PropertyType.IsListOrDictionaryType())
                .OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                .Concat(allMembers.Where(x => x is FieldInfo)); // add FieldInfo objects back

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

        private static bool PropertyReadable(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead;
        }

        private static bool PropertyWritable(PropertyInfo propertyInfo)
        {
            bool propertyIsEnumerable = (typeof (string) != propertyInfo.PropertyType)
                                        && typeof (IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);

            return propertyInfo.CanWrite || propertyIsEnumerable;
        }

        private IEnumerable<MemberInfo> GetAllPublicMembers(Func<PropertyInfo, bool> propertyAvailableFor,
            Func<MemberInfo, bool> memberAvailableFor)
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
                .Where(m => (m.ReturnType != typeof (void)) && (m.GetParameters().Length == 0))
                .ToArray();
        }
    }
}