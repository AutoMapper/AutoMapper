namespace AutoMapper.UnitTests.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Should;
    using Xunit;
    using Internal;

    public class StubNamingConvention : INamingConvention
    {
        public Regex SplittingExpression { get; set; }
        public string SeparatorCharacter { get; set; }
    }

    // ReSharper disable ConvertPropertyToExpressionBody
    // ReSharper disable ConvertToAutoProperty
    public class StubMappingOptions : IMappingOptions
    {
        private INamingConvention _sourceMemberNamingConvention;

        private INamingConvention _destinationMemberNamingConvention;

        private readonly IEnumerable<string> _prefixes = new List<string>();

        private readonly IEnumerable<string> _postfixes = new List<string>();

        private readonly IEnumerable<string> _destinationPrefixes = new List<string>();

        private readonly IEnumerable<string> _destinationPostfixes = new List<string>();

        private readonly IEnumerable<AliasedMember> _aliases = new List<AliasedMember>();

        private readonly HashSet<MemberNameReplacer> _memberNameReplacers = new HashSet<MemberNameReplacer>();

        private readonly IEnumerable<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

        public INamingConvention SourceMemberNamingConvention
        {
            get { return _sourceMemberNamingConvention; }
            set { _sourceMemberNamingConvention = value; }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get { return _destinationMemberNamingConvention; }
            set { _destinationMemberNamingConvention = value; }
        }

        public IEnumerable<string> Prefixes
        {
            get { return _prefixes; }
        }

        public IEnumerable<string> Postfixes
        {
            get { return _postfixes; }
        }

        public IEnumerable<string> DestinationPrefixes
        {
            get { return _destinationPrefixes; }
        }

        public IEnumerable<string> DestinationPostfixes
        {
            get { return _destinationPostfixes; }
        }

        public IEnumerable<MemberNameReplacer> MemberNameReplacers
        {
            get { return _memberNameReplacers; }
        }

        public IEnumerable<AliasedMember> Aliases
        {
            get { return _aliases; }
        }

        public bool ConstructorMappingEnabled
        {
            get { return true; }
        }

        public bool DataReaderMapperYieldReturnEnabled
        {
            get { return false; }
        }

        public IEnumerable<MethodInfo> SourceExtensionMethods
        {
            get { return _sourceExtensionMethods; }
        }

        public void ReplaceMemberName(string original, string newValue)
        {
            _memberNameReplacers.Add(new MemberNameReplacer(original, newValue));
        }
    }

    public class When_constructing_type_maps_with_matching_property_names : SpecBase
    {
        private TypeMapFactory _factory;

        public class Source
        {
            public int Value { get; set; }
            public int SomeOtherValue { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
            public int SomeOtherValue { get; set; }
        }

        protected override void Establish_context()
        {
            _factory = new TypeMapFactory();
        }

        [Fact]
        public void Should_map_properties_with_same_name()
        {
            var mappingOptions = new StubMappingOptions
            {
                SourceMemberNamingConvention = new PascalCaseNamingConvention(),
                DestinationMemberNamingConvention = new PascalCaseNamingConvention()
            };

            var typeMap = _factory.CreateTypeMap(typeof (Source), typeof (Destination), mappingOptions,
                MemberList.Destination);

            var propertyMaps = typeMap.GetPropertyMaps();

            propertyMaps.Count().ShouldEqual(2);
        }
    }

    public class When_using_a_custom_source_naming_convention : SpecBase
    {
        private TypeMapFactory _factory;
        private TypeMap _map;
        private StubMappingOptions _mappingOptions;


        private class Source
        {
            public SubSource some__source { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SubSource
        {
            public int value { get; set; }
        }

        private class Destination
        {
            public int SomeSourceValue { get; set; }
        }

        protected override void Establish_context()
        {
            var namingConvention = new StubNamingConvention {SeparatorCharacter = "__"};

            _mappingOptions = new StubMappingOptions
            {
                SourceMemberNamingConvention = namingConvention,
                DestinationMemberNamingConvention = new PascalCaseNamingConvention()
            };

            _factory = new TypeMapFactory();

        }

        protected override void Because_of()
        {
            _map = _factory.CreateTypeMap(typeof (Source), typeof (Destination), _mappingOptions, MemberList.Destination);
        }

        [Fact]
        public void Should_split_using_naming_convention_rules()
        {
            _map.GetPropertyMaps().Count().ShouldEqual(1);
        }
    }

    public class When_using_a_custom_destination_naming_convention : SpecBase
    {
        private TypeMapFactory _factory;
        private TypeMap _map;
        private StubMappingOptions _mappingOptions;

        private class Source
        {
            public SubSource SomeSource { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SubSource
        {
            public int Value { get; set; }
        }

        private class Destination
        {
            public int some__source__value { get; set; }
        }

        protected override void Establish_context()
        {
            var namingConvention = new StubNamingConvention
            {
                SplittingExpression = new Regex(@"[\p{Ll}0-9]*(?=_?)")
            };

            _mappingOptions = new StubMappingOptions
            {
                SourceMemberNamingConvention = new PascalCaseNamingConvention(),
                DestinationMemberNamingConvention = namingConvention
            };

            _factory = new TypeMapFactory();
        }

        protected override void Because_of()
        {
            _map = _factory.CreateTypeMap(typeof (Source), typeof (Destination),
                _mappingOptions, MemberList.Destination);
        }

        [Fact]
        public void Should_split_using_naming_convention_rules()
        {
            _map.GetPropertyMaps().Count().ShouldEqual(1);
        }
    }

    public class When_using_a_source_member_name_replacer : SpecBase
    {
        private TypeMapFactory _factory;

        public class Source
        {
            public int Value { get; set; }
            public int Ävíator { get; set; }
            public int SubAirlinaFlight { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
            public int Aviator { get; set; }
            public int SubAirlineFlight { get; set; }
        }

        protected override void Establish_context()
        {
            _factory = new TypeMapFactory();
        }

        [Fact]
        public void Should_map_properties_with_different_names()
        {
            var mappingOptions = new StubMappingOptions();
            mappingOptions.ReplaceMemberName("Ä", "A");
            mappingOptions.ReplaceMemberName("í", "i");
            mappingOptions.ReplaceMemberName("Airlina", "Airline");

            var typeMap = _factory.CreateTypeMap(typeof (Source), typeof (Destination), mappingOptions,
                MemberList.Destination);

            var propertyMaps = typeMap.GetPropertyMaps();

            propertyMaps.Count().ShouldEqual(3);
        }
    }
}