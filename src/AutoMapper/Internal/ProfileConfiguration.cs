namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class ProfileConfiguration : IProfileConfiguration, IMappingOptions
    {
        private readonly ISet<string> _prefixes = new HashSet<string>();
        private readonly ISet<string> _postfixes = new HashSet<string>();
        private readonly ISet<string> _destinationPrefixes = new HashSet<string>();
        private readonly ISet<string> _destinationPostfixes = new HashSet<string>();
        private readonly ISet<AliasedMember> _aliases = new HashSet<AliasedMember>();
        private readonly ISet<MemberNameReplacer> _memberNameReplacers = new HashSet<MemberNameReplacer>();
        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

        public ProfileConfiguration()
        {
            SourceMemberNamingConvention = new PascalCaseNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
            RecognizePrefixes("Get");
            AllowNullDestinationValues = true;
            ConstructorMappingEnabled = true;
            IncludeSourceExtensionMethods(typeof (Enumerable).Assembly());
            ShouldMapProperty = p => p.IsPublic();
            ShouldMapField = f => f.IsPublic;
        }

        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; }

        public Func<FieldInfo, bool> ShouldMapField { get; set; }

        public bool AllowNullDestinationValues { get; set; }
        public bool AllowNullCollections { get; set; }
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }

        public IEnumerable<string> Prefixes => _prefixes;

        public IEnumerable<string> Postfixes => _postfixes;

        public IEnumerable<string> DestinationPrefixes => _destinationPrefixes;

        public IEnumerable<string> DestinationPostfixes => _destinationPostfixes;

        public IEnumerable<MemberNameReplacer> MemberNameReplacers => _memberNameReplacers;

        public IEnumerable<AliasedMember> Aliases => _aliases;

        public bool ConstructorMappingEnabled { get; set; }
        public bool DataReaderMapperYieldReturnEnabled { get; set; }

        public IEnumerable<MethodInfo> SourceExtensionMethods => _sourceExtensionMethods;


        public bool MapNullSourceValuesAsNull => AllowNullDestinationValues;

        public bool MapNullSourceCollectionsAsNull => AllowNullCollections;

        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            //http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
            _sourceExtensionMethods.AddRange(assembly.GetTypes()
                .Where(type => type.IsSealed() && !type.IsGenericType() && !type.IsNested)
                .SelectMany(type => type.GetDeclaredMethods().Where(mi => mi.IsStatic))
                .Where(method => method.IsDefined(typeof (ExtensionAttribute), false))
                .Where(method => method.GetParameters().Length == 1));
        }

        public void ClearPrefixes()
        {
            _prefixes.Clear();
        }

        public void RecognizePrefixes(params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                _prefixes.Add(prefix);
            }
        }

        public void RecognizePostfixes(params string[] postfixes)
        {
            foreach (var postfix in postfixes)
            {
                _postfixes.Add(postfix);
            }
        }

        public void RecognizeAlias(string original, string alias)
        {
            _aliases.Add(new AliasedMember(original, alias));
        }

        public void ReplaceMemberName(string original, string newValue)
        {
            _memberNameReplacers.Add(new MemberNameReplacer(original, newValue));
        }

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                _destinationPrefixes.Add(prefix);
            }
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            foreach (var postfix in postfixes)
            {
                _destinationPostfixes.Add(postfix);
            }
        }
    }
}