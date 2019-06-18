using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoMapper.Configuration;

namespace AutoMapper
{
    /// <summary>
    /// Contains cached reflection information for easy retrieval
    /// </summary>
    [DebuggerDisplay("{Type}")]
    public class TypeDetails
    {
        public TypeDetails(Type type, ProfileMap config)
        {
            Type = type;
            var membersToMap = MembersToMap(config.ShouldMapProperty, config.ShouldMapField);
            var publicReadableMembers = GetAllPublicReadableMembers(membersToMap);
            var publicWritableMembers = GetAllPublicWritableMembers(membersToMap);
            PublicReadAccessors = BuildPublicReadAccessors(publicReadableMembers);
            PublicWriteAccessors = BuildPublicAccessors(publicWritableMembers);
            PublicNoArgMethods = BuildPublicNoArgMethods(config.ShouldMapMethod);
            Constructors = GetAllConstructors(config.ShouldUseConstructor);
            PublicNoArgExtensionMethods = BuildPublicNoArgExtensionMethods(config.SourceExtensionMethods.Where(config.ShouldMapMethod));
            AllMembers = PublicReadAccessors.Concat(PublicNoArgMethods).Concat(PublicNoArgExtensionMethods).ToList();
            DestinationMemberNames = AllMembers.Select(mi => new DestinationMemberName { Member = mi, Possibles = PossibleNames(mi.Name, config.Prefixes, config.Postfixes).ToArray() });
        }

        private IEnumerable<string> PossibleNames(string memberName, IEnumerable<string> prefixes, IEnumerable<string> postfixes)
        {
            yield return memberName;

            if (!postfixes.Any())
            {
                foreach (var withoutPrefix in prefixes.Where(prefix => memberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).Select(prefix => memberName.Substring(prefix.Length)))
                {
                    yield return withoutPrefix;
                }
                yield break;
            }

            foreach (var withoutPrefix in prefixes.Where(prefix => memberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).Select(prefix => memberName.Substring(prefix.Length)))
            {
                yield return withoutPrefix;
                foreach (var s in PostFixes(postfixes, withoutPrefix))
                    yield return s;
            }
            foreach (var s in PostFixes(postfixes, memberName))
                yield return s;
        }

        private static IEnumerable<string> PostFixes(IEnumerable<string> postfixes, string name)
        {
            return
                postfixes.Where(postfix => name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
                    .Select(postfix => name.Remove(name.Length - postfix.Length));
        }

        private static Func<MemberInfo, bool> MembersToMap(
            Func<PropertyInfo, bool> shouldMapProperty, 
            Func<FieldInfo, bool> shouldMapField)
        {
            return m =>
            {
                switch (m)
                {
                    case PropertyInfo property:
                        return !property.IsStatic() && shouldMapProperty(property);
                    case FieldInfo field:
                        return !field.IsStatic && shouldMapField(field);
                    default:
                        throw new ArgumentException("Should be a field or a property.");
                }
            };
        }

        public struct DestinationMemberName
        {
            public MemberInfo Member { get; set; }
            public string[] Possibles { get; set; }
        }

        public Type Type { get; }

        public IEnumerable<ConstructorInfo> Constructors { get; }

        public IEnumerable<MemberInfo> PublicReadAccessors { get; }

        public IEnumerable<MemberInfo> PublicWriteAccessors { get; }

        public IEnumerable<MethodInfo> PublicNoArgMethods { get; }

        public IEnumerable<MethodInfo> PublicNoArgExtensionMethods { get; }

        public IEnumerable<MemberInfo> AllMembers { get; }

        public IEnumerable<DestinationMemberName> DestinationMemberNames { get; set; }

        private IEnumerable<MethodInfo> BuildPublicNoArgExtensionMethods(IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            var explicitExtensionMethods = sourceExtensionMethodSearch.Where(method => method.GetParameters()[0].ParameterType == Type);

            var genericInterfaces = Type.GetTypeInfo().ImplementedInterfaces.Where(t => t.IsGenericType());

            if (Type.IsInterface() && Type.IsGenericType())
            {
                genericInterfaces = genericInterfaces.Union(new[] { Type });
            }

            return explicitExtensionMethods.Union
            (
                from genericInterface in genericInterfaces
                let genericInterfaceArguments = genericInterface.GetTypeInfo().GenericTypeArguments
                let matchedMethods = (
                    from extensionMethod in sourceExtensionMethodSearch
                    where !extensionMethod.IsGenericMethodDefinition
                    select extensionMethod
                ).Concat(
                    from extensionMethod in sourceExtensionMethodSearch
                    where extensionMethod.IsGenericMethodDefinition
                        && extensionMethod.GetGenericArguments().Length == genericInterfaceArguments.Length
                    select extensionMethod.MakeGenericMethod(genericInterfaceArguments)
                )
                from methodMatch in matchedMethods
                where methodMatch.GetParameters()[0].ParameterType.GetTypeInfo().IsAssignableFrom(genericInterface.GetTypeInfo())
                select methodMatch
            ).ToArray();
        }

        private static MemberInfo[] BuildPublicReadAccessors(IEnumerable<MemberInfo> allMembers)
        {
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var filteredMembers = allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
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
                //.OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                .Concat(allMembers.Where(x => x is FieldInfo)); // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

        private IEnumerable<MemberInfo> GetAllPublicReadableMembers(Func<MemberInfo, bool> membersToMap) 
            => GetAllPublicMembers(PropertyReadable, FieldReadable, membersToMap);

        private IEnumerable<MemberInfo> GetAllPublicWritableMembers(Func<MemberInfo, bool> membersToMap)
            => GetAllPublicMembers(PropertyWritable, FieldWritable, membersToMap);
        
        private IEnumerable<ConstructorInfo> GetAllConstructors(Func<ConstructorInfo, bool> shouldUseConstructor)
        {
            return Type.GetDeclaredConstructors().Where(shouldUseConstructor).ToArray();
        }

        private static bool PropertyReadable(PropertyInfo propertyInfo) => propertyInfo.CanRead;

        private static bool FieldReadable(FieldInfo fieldInfo) => true;

        private static bool PropertyWritable(PropertyInfo propertyInfo)
        {
            var propertyIsEnumerable = (typeof(string) != propertyInfo.PropertyType)
                                        && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo());

            return propertyInfo.CanWrite || propertyIsEnumerable;
        }

        private static bool FieldWritable(FieldInfo fieldInfo) => !fieldInfo.IsInitOnly;

        private IEnumerable<MemberInfo> GetAllPublicMembers(
            Func<PropertyInfo, bool> propertyAvailableFor,
            Func<FieldInfo, bool> fieldAvailableFor,
            Func<MemberInfo, bool> memberAvailableFor)
        {
            var typesToScan = new List<Type>();
            for (var t = Type; t != null; t = t.BaseType())
                typesToScan.Add(t);

            if (Type.IsInterface())
                typesToScan.AddRange(Type.GetTypeInfo().ImplementedInterfaces);

            // Scan all types for public properties and fields
            return typesToScan
                .Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
                .SelectMany(x => x.GetDeclaredMembers()
                    .Where(mi => mi.DeclaringType != null && mi.DeclaringType == x)
                    .Where(
                        m =>
                            m is FieldInfo && fieldAvailableFor((FieldInfo)m) ||
                            m is PropertyInfo && propertyAvailableFor((PropertyInfo)m) &&
                            !((PropertyInfo)m).GetIndexParameters().Any())
                    .Where(memberAvailableFor)
                );
        }

        private MethodInfo[] BuildPublicNoArgMethods(Func<MethodInfo, bool> shouldMapMethod)
        {
            return Type.GetAllMethods()
                .Where(shouldMapMethod)
                .Where(mi => mi.IsPublic && !mi.IsStatic && mi.DeclaringType != typeof(object))
                .Where(m => (m.ReturnType != typeof(void)) && (m.GetParameters().Length == 0))
                .ToArray();
        }
    }
}
