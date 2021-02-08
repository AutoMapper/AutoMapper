using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    /// <summary>
    /// Contains cached reflection information for easy retrieval
    /// </summary>
    [DebuggerDisplay("{Type}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeDetails
    {
        private readonly Dictionary<string, MemberInfo> _nameToMember;
        private readonly MemberInfo[] _allMembers;

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
            _allMembers = PublicReadAccessors.Concat(PublicNoArgMethods).Concat(PublicNoArgExtensionMethods).ToArray();
            _nameToMember = new Dictionary<string, MemberInfo>(_allMembers.Length, StringComparer.OrdinalIgnoreCase);
            PossibleNames(config);
        }
        public MemberInfo GetMember(string name) => _nameToMember.GetOrDefault(name);
        private void PossibleNames(ProfileMap config)
        {
            foreach (var member in _allMembers)
            {
                foreach (var memberName in PossibleNames(member.Name, config.Prefixes, config.Postfixes))
                {
                    _nameToMember[memberName] = member;
                }
            }
        }
        public static IEnumerable<string> PossibleNames(string memberName, List<string> prefixes, List<string> postfixes)
        {
            yield return memberName;
            foreach (var prefix in prefixes)
            {
                if (!memberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var withoutPrefix = memberName.Substring(prefix.Length);
                yield return withoutPrefix;
                foreach (var s in PostFixes(postfixes, withoutPrefix))
                {
                    yield return s;
                }
            }
            foreach (var s in PostFixes(postfixes, memberName))
            {
                yield return s;
            }
        }

        private static IEnumerable<string> PostFixes(List<string> postfixes, string name)
        {
            foreach (var postfix in postfixes)
            {
                if (!name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                yield return name.Remove(name.Length - postfix.Length);
            }
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

        public Type Type { get; }

        public IEnumerable<ConstructorInfo> Constructors { get; }

        public IEnumerable<MemberInfo> PublicReadAccessors { get; }

        public IEnumerable<MemberInfo> PublicWriteAccessors { get; }

        public IEnumerable<MethodInfo> PublicNoArgMethods { get; }

        public IEnumerable<MethodInfo> PublicNoArgExtensionMethods { get; }

        public IEnumerable<MemberInfo> AllMembers => _allMembers;

        private IEnumerable<MethodInfo> BuildPublicNoArgExtensionMethods(IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            var extensionMethodSearch = sourceExtensionMethodSearch as MethodInfo[] ?? sourceExtensionMethodSearch.ToArray();
            var explicitExtensionMethods = extensionMethodSearch.Where(method => method.GetParameters()[0].ParameterType.IsAssignableFrom(Type));

            var genericInterfaces = Type.GetTypeInfo().ImplementedInterfaces.Where(t => t.IsGenericType);

            if (Type.IsInterface && Type.IsGenericType)
            {
                genericInterfaces = genericInterfaces.Union(new[] { Type });
            }

            return explicitExtensionMethods.Union
            (
                from genericInterface in genericInterfaces
                let genericInterfaceArguments = genericInterface.GenericTypeArguments
                let matchedMethods = (
                    from extensionMethod in extensionMethodSearch
                    where !extensionMethod.IsGenericMethodDefinition
                    select extensionMethod
                ).Concat(
                    from extensionMethod in extensionMethodSearch
                    where extensionMethod.IsGenericMethodDefinition
                        && extensionMethod.GetGenericArguments().Length == genericInterfaceArguments.Length
                    select extensionMethod.MakeGenericMethod(genericInterfaceArguments)
                )
                from methodMatch in matchedMethods
                where methodMatch.GetParameters()[0].ParameterType.IsAssignableFrom(genericInterface)
                select methodMatch
            ).ToArray();
        }

        private static MemberInfo[] BuildPublicReadAccessors(IEnumerable<MemberInfo> allMembers)
        {
            var memberInfos = allMembers as MemberInfo[] ?? allMembers.ToArray();
            return memberInfos
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
                .Concat(memberInfos.Where(x => x is FieldInfo)) // add FieldInfo objects back
                .ToArray();
        }

        private static MemberInfo[] BuildPublicAccessors(IEnumerable<MemberInfo> allMembers)
        {
            var memberInfos = allMembers as MemberInfo[] ?? allMembers.ToArray();
            return memberInfos
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x =>
                    x.FirstOrDefault(y => y.CanWrite && y.CanRead) ??
                    x.First()) // favor the first property that can both read & write - otherwise pick the first one
                .Concat(memberInfos.Where(x => x is FieldInfo)) // add FieldInfo objects back
                .ToArray();
        }

        private IEnumerable<MemberInfo> GetAllPublicReadableMembers(Func<MemberInfo, bool> membersToMap)
            => GetAllPublicMembers(PropertyReadable, FieldReadable, membersToMap);

        private IEnumerable<MemberInfo> GetAllPublicWritableMembers(Func<MemberInfo, bool> membersToMap)
            => GetAllPublicMembers(PropertyWritable, FieldWritable, membersToMap);

        private IEnumerable<ConstructorInfo> GetAllConstructors(Func<ConstructorInfo, bool> shouldUseConstructor)
            => Type.GetDeclaredConstructors().Where(shouldUseConstructor).ToArray();

        private static bool PropertyReadable(PropertyInfo propertyInfo) => propertyInfo.CanRead;

        private static bool FieldReadable(FieldInfo fieldInfo) => true;

        private static bool PropertyWritable(PropertyInfo propertyInfo) => propertyInfo.CanWrite || propertyInfo.PropertyType.IsNonStringEnumerable();

        private static bool FieldWritable(FieldInfo fieldInfo) => !fieldInfo.IsInitOnly;

        private IEnumerable<MemberInfo> GetAllPublicMembers(
            Func<PropertyInfo, bool> propertyAvailableFor,
            Func<FieldInfo, bool> fieldAvailableFor,
            Func<MemberInfo, bool> memberAvailableFor)
        {
            var typesToScan = new List<Type>();
            for (var t = Type; t != null; t = t.BaseType)
                typesToScan.Add(t);

            if (Type.IsInterface)
                typesToScan.AddRange(Type.GetTypeInfo().ImplementedInterfaces);

            // Scan all types for public properties and fields
            return typesToScan
                .Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
                .SelectMany(x => x.GetDeclaredMembers()
                    .Where(mi => mi.DeclaringType != null && mi.DeclaringType == x)
                    .Where(
                        m =>
                            m is FieldInfo info && fieldAvailableFor(info) ||
                            m is PropertyInfo propertyInfo && propertyAvailableFor(propertyInfo) &&
                            !propertyInfo.GetIndexParameters().Any())
                    .Where(memberAvailableFor)
                );
        }

        private MethodInfo[] BuildPublicNoArgMethods(Func<MethodInfo, bool> shouldMapMethod)
        {
            return Type.GetRuntimeMethods()
                .Where(shouldMapMethod)
                .Where(mi => mi.IsPublic && !mi.IsStatic && mi.DeclaringType != typeof(object))
                .Where(m => (m.ReturnType != typeof(void)) && (m.GetParameters().Length == 0))
                .ToArray();
        }
    }
}