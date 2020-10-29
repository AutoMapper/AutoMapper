using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    using SourceMembers = Dictionary<string, MemberInfo>;

    /// <summary>
    /// Contains cached reflection information for easy retrieval
    /// </summary>
    [DebuggerDisplay("{Type}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeDetails
    {
        private readonly Lazy<SourceMembers> _nameToMember;
        private readonly Lazy<MemberInfo[]> _readAccessors;
        private readonly Lazy<MemberInfo[]> _writeAccessors;
        public TypeDetails(Type type, ProfileMap config)
        {
            Type = type;
            Config = config;
            _readAccessors = new Lazy<MemberInfo[]>(BuildReadAccessors);
            _writeAccessors = new Lazy<MemberInfo[]>(BuildWriteAccessors);
            _nameToMember = new Lazy<SourceMembers>(PossibleNames, isThreadSafe: false);
        }
        public MemberInfo GetMember(string name) => _nameToMember.Value.GetOrDefault(name);
        private SourceMembers PossibleNames()
        {
            var nameToMember = new SourceMembers(ReadAccessors.Length, StringComparer.OrdinalIgnoreCase);
            var publicNoArgMethods = GetPublicNoArgMethods();
            var publicNoArgExtensionMethods = GetPublicNoArgExtensionMethods(Config.SourceExtensionMethods.Where(Config.ShouldMapMethod));
            foreach (var member in ReadAccessors.Concat(publicNoArgMethods).Concat(publicNoArgExtensionMethods))
            {
                foreach (var memberName in PossibleNames(member.Name, Config.Prefixes, Config.Postfixes))
                {
                    if (!nameToMember.ContainsKey(memberName))
                    {
                        nameToMember.Add(memberName, member);
                    }
                }
            }
            return nameToMember;
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
        public Type Type { get; }
        public ProfileMap Config { get; }
        public MemberInfo[] ReadAccessors => _readAccessors.Value;
        public MemberInfo[] WriteAccessors => _writeAccessors.Value;
        private IEnumerable<MethodInfo> GetPublicNoArgExtensionMethods(IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            var explicitExtensionMethods = sourceExtensionMethodSearch.Where(method => method.GetParameters()[0].ParameterType.IsAssignableFrom(Type));
            var genericInterfaces = Type.GetInterfaces().Where(t => t.IsGenericType);
            if (Type.IsInterface && Type.IsGenericType)
            {
                genericInterfaces = genericInterfaces.Union(new[] { Type });
            }
            return explicitExtensionMethods.Union
            (
                from genericInterface in genericInterfaces
                let genericInterfaceArguments = genericInterface.GenericTypeArguments
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
                where methodMatch.GetParameters()[0].ParameterType.IsAssignableFrom(genericInterface)
                select methodMatch
            );
        }
        private MemberInfo[] BuildReadAccessors() =>
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            GetProperties(PropertyReadable)
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
                .Concat(GetFields(FieldReadable))
                .ToArray();
        private MemberInfo[] BuildWriteAccessors() =>
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            GetProperties(PropertyWritable)
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.FirstOrDefault(y => y.CanWrite && y.CanRead) ?? x.First()) // favor the first property that can both read & write - otherwise pick the first one
                .Concat(GetFields(FieldWritable))
                .ToArray();
        private static bool PropertyReadable(PropertyInfo propertyInfo) => propertyInfo.CanRead;
        private static bool FieldReadable(FieldInfo fieldInfo) => true;
        private static bool PropertyWritable(PropertyInfo propertyInfo) => propertyInfo.CanWrite || propertyInfo.PropertyType.IsNonStringEnumerable();
        private static bool FieldWritable(FieldInfo fieldInfo) => !fieldInfo.IsInitOnly;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private IEnumerable<Type> GetTypeInheritance() => Type.GetTypeInheritance().Concat(Type.IsInterface ? Type.GetInterfaces() : Type.EmptyTypes);
        private IEnumerable<PropertyInfo> GetProperties(Func<PropertyInfo, bool> propertyAvailableFor) =>
            GetTypeInheritance().SelectMany(type => type.GetProperties(Flags).Where(property => propertyAvailableFor(property) && Config.ShouldMapProperty(property)));
        private IEnumerable<MemberInfo> GetFields(Func<FieldInfo, bool> fieldAvailableFor) =>
            GetTypeInheritance().SelectMany(type => type.GetFields(Flags).Where(field => fieldAvailableFor(field) && Config.ShouldMapField(field)));
        private IEnumerable<MethodInfo> GetPublicNoArgMethods() =>
            Type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.DeclaringType != typeof(object) && m.ReturnType != typeof(void) && Config.ShouldMapMethod(m) && m.GetParameters().Length == 0);
    }
}