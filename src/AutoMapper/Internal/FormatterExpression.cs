using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    using Internal;
    using System.Runtime.CompilerServices;

    public class FormatterExpression : IProfileConfiguration, IMappingOptions
    {
		private readonly IList<Type> _formattersToSkip = new List<Type>();
	    private readonly ISet<string> _prefixes = new HashSet<string>();
        private readonly ISet<string> _postfixes = new HashSet<string>();
        private readonly ISet<string> _destinationPrefixes = new HashSet<string>();
        private readonly ISet<string> _destinationPostfixes = new HashSet<string>();
        private readonly ISet<AliasedMember> _aliases = new HashSet<AliasedMember>();
        private readonly ISet<MemberNameReplacer> _memberNameReplacers = new HashSet<MemberNameReplacer>();
        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

	    public FormatterExpression()
		{
			SourceMemberNamingConvention = new PascalCaseNamingConvention();
			DestinationMemberNamingConvention = new PascalCaseNamingConvention();
		    RecognizePrefixes("Get");
			AllowNullDestinationValues = true;
	        ConstructorMappingEnabled = true;
            IncludeSourceExtensionMethods(typeof(Enumerable).Assembly());
		}

		public bool AllowNullDestinationValues { get; set; }
		public bool AllowNullCollections { get; set; }
		public INamingConvention SourceMemberNamingConvention { get; set; }
		public INamingConvention DestinationMemberNamingConvention { get; set; }
        public IEnumerable<string> Prefixes { get { return _prefixes; } }
        public IEnumerable<string> Postfixes { get { return _postfixes; } }
        public IEnumerable<string> DestinationPrefixes { get { return _destinationPrefixes; } }
        public IEnumerable<string> DestinationPostfixes { get { return _destinationPostfixes; } }
        public IEnumerable<MemberNameReplacer> MemberNameReplacers { get { return _memberNameReplacers; } }
        public IEnumerable<AliasedMember> Aliases { get { return _aliases; } }
        public bool ConstructorMappingEnabled { get; set; }
        public bool DataReaderMapperYieldReturnEnabled { get; set; }
        public IEnumerable<MethodInfo> SourceExtensionMethods { get { return _sourceExtensionMethods; } }


		public Type[] GetFormatterTypesToSkip()
		{
			return _formattersToSkip.ToArray();
		}

		public bool MapNullSourceValuesAsNull
		{
			get { return AllowNullDestinationValues; }
		}

		public bool MapNullSourceCollectionsAsNull
		{
			get { return AllowNullCollections; }
		}

        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            //http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
            _sourceExtensionMethods.AddRange(assembly.GetTypes()
                .Where(type => type.IsSealed() && !type.IsGenericType() && !type.IsNested)
                .SelectMany(type => type.GetDeclaredMethods().Where(mi => mi.IsStatic))
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.GetParameters().Length == 1));
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