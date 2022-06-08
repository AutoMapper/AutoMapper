using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
namespace AutoMapper.Internal
{
    using SourceMembers = Dictionary<string, MemberInfo>;
    /// <summary>
    /// Contains cached reflection information for easy retrieval
    /// </summary>
    [DebuggerDisplay("{Type}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeDetails
    {
        private SourceMembers _nameToMember;
        private ConstructorParameters[] _constructors;
        private MemberInfo[] _readAccessors;
        private MemberInfo[] _writeAccessors;
        public TypeDetails(Type type, ProfileMap config)
        {
            Type = type;
            Config = config;
        }
        private ConstructorParameters[] GetConstructors() => 
            GetConstructors(Type, Config).Where(c=>c.ParametersCount > 0).OrderByDescending(c => c.ParametersCount).ToArray();
        public static IEnumerable<ConstructorParameters> GetConstructors(Type type, ProfileMap profileMap) =>
            type.GetDeclaredConstructors().Where(profileMap.ShouldUseConstructor).Select(c => new ConstructorParameters(c));
        public MemberInfo GetMember(string name)
        {
            _nameToMember ??= PossibleNames();
            return _nameToMember.GetValueOrDefault(name);
        }
        private SourceMembers PossibleNames()
        {
            var nameToMember = new SourceMembers(ReadAccessors.Length, StringComparer.OrdinalIgnoreCase);
            IEnumerable<MemberInfo> accessors = ReadAccessors;
            if (Config.MethodMappingEnabled)
            {
                accessors = AddMethods(accessors);
            }
            foreach (var member in accessors)
            {
                if (!nameToMember.TryAdd(member.Name, member))
                {
                    continue;
                }
                if (Config.Postfixes.Count == 0 && Config.Prefixes.Count == 0)
                {
                    continue;
                }
                CheckPrePostfixes(nameToMember, member);
            }
            return nameToMember;
        }
        private IEnumerable<MemberInfo> AddMethods(IEnumerable<MemberInfo> accessors)
        {
            var publicNoArgMethods = GetPublicNoArgMethods();
            var publicNoArgExtensionMethods = GetPublicNoArgExtensionMethods(Config.SourceExtensionMethods.Where(Config.ShouldMapMethod));
            return accessors.Concat(publicNoArgMethods).Concat(publicNoArgExtensionMethods);
        }
        private void CheckPrePostfixes(SourceMembers nameToMember, MemberInfo member)
        {
            foreach (var memberName in PossibleNames(member.Name, Config.Prefixes, Config.Postfixes))
            {
                nameToMember.TryAdd(memberName, member);
            }
        }
        public static IEnumerable<string> PossibleNames(string memberName, List<string> prefixes, List<string> postfixes)
        {
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
        public MemberInfo[] ReadAccessors => _readAccessors ??= BuildReadAccessors();
        public MemberInfo[] WriteAccessors => _writeAccessors ??= BuildWriteAccessors();
        public ConstructorParameters[] Constructors => _constructors ??= GetConstructors();
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
                    let constructedGeneric = MakeGenericMethod(extensionMethod, genericInterfaceArguments)
                    where constructedGeneric != null
                    select constructedGeneric
                )
                from methodMatch in matchedMethods
                where methodMatch.GetParameters()[0].ParameterType.IsAssignableFrom(genericInterface)
                select methodMatch
            );

            // Use method.MakeGenericMethod(genericArguments) wrapped in a try/catch(ArgumentException)
            // in order to catch exceptions resulting from the generic arguments not being compatible
            // with any constraints that may be on the generic method's generic parameters.
            static MethodInfo MakeGenericMethod(MethodInfo genericMethod, Type[] genericArguments)
            {
                try
                {
                    return genericMethod.MakeGenericMethod(genericArguments);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }
        private MemberInfo[] BuildReadAccessors()
        {
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            IEnumerable<MemberInfo> members = GetProperties(PropertyReadable)
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First());
            if (Config.FieldMappingEnabled)
            {
                members = members.Concat(GetFields(FieldReadable));
            }
            return members.ToArray();
        }
        private MemberInfo[] BuildWriteAccessors()
        {
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            IEnumerable<MemberInfo> members = GetProperties(PropertyWritable)
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.FirstOrDefault(y => y.CanWrite && y.CanRead) ?? x.First()); // favor the first property that can both read & write - otherwise pick the first one
            if (Config.FieldMappingEnabled)
            {
                members = members.Concat(GetFields(FieldWritable));
            }
            return members.ToArray();
        }
        private static bool PropertyReadable(PropertyInfo propertyInfo) => propertyInfo.CanRead;
        private static bool FieldReadable(FieldInfo fieldInfo) => true;
        private static bool PropertyWritable(PropertyInfo propertyInfo) => propertyInfo.CanWrite || propertyInfo.PropertyType.IsCollection();
        private static bool FieldWritable(FieldInfo fieldInfo) => !fieldInfo.IsInitOnly;
        private IEnumerable<Type> GetTypeInheritance() => Type.IsInterface ? new[] { Type }.Concat(Type.GetInterfaces()) : Type.GetTypeInheritance();
        private IEnumerable<PropertyInfo> GetProperties(Func<PropertyInfo, bool> propertyAvailableFor) =>
            GetTypeInheritance().SelectMany(type => type.GetProperties(TypeExtensions.InstanceFlags).Where(property => propertyAvailableFor(property) && Config.ShouldMapProperty(property)));
        private IEnumerable<MemberInfo> GetFields(Func<FieldInfo, bool> fieldAvailableFor) =>
            GetTypeInheritance().SelectMany(type => type.GetFields(TypeExtensions.InstanceFlags).Where(field => fieldAvailableFor(field) && Config.ShouldMapField(field)));
        private IEnumerable<MethodInfo> GetPublicNoArgMethods() =>
            Type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.DeclaringType != typeof(object) && m.ReturnType != typeof(void) && Config.ShouldMapMethod(m) && m.GetParameters().Length == 0);
    }
    public readonly struct ConstructorParameters
    {
        public readonly ConstructorInfo Constructor;
        public readonly ParameterInfo[] Parameters;
        public ConstructorParameters(ConstructorInfo constructor)
        {
            Constructor = constructor;
            Parameters = constructor.GetParameters();
        }
        public int ParametersCount => Parameters.Length;
        public bool AllParametersOptional() => Parameters.All(p => p.IsOptional);
    }
}