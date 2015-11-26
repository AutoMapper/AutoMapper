using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper.Internal;
using Xunit;
using Should;

namespace AutoMapper.UnitTests.Tests
{
    using System;
    using Assembly = System.Reflection.Assembly;

    public class StubNamingConvention : INamingConvention
    {
        private readonly Func<Match, string> _replaceFunc;

        public StubNamingConvention(Func<Match, string> replaceFunc)
        {
            _replaceFunc = replaceFunc;
            SeparatorCharacter = "";
        }

        public Regex SplittingExpression { get; set; }
        public string SeparatorCharacter { get; set; }

        public string ReplaceValue(Match match)
        {
            return _replaceFunc(match);
        }
    }

    public class StubMappingOptions : IMappingOptions
    {
        private INamingConvention _sourceMemberNamingConvention;

        private INamingConvention _destinationMemberNamingConvention;

        private IEnumerable<string> _prefixes = new List<string>();

        private IEnumerable<string> _postfixes = new List<string>();

        private IEnumerable<string> _destinationPrefixes = new List<string>();

        private IEnumerable<string> _destinationPostfixes = new List<string>();

        private IEnumerable<AliasedMember> _aliases = new List<AliasedMember>();

        private HashSet<MemberNameReplacer> _memberNameReplacers = new HashSet<MemberNameReplacer>();

        private IEnumerable<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

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

        public IEnumerable<MethodInfo> SourceExtensionMethods
        {
            get { return _sourceExtensionMethods; }
        }

        public Func<PropertyInfo, bool> ShouldMapProperty
        {
            get
            {
                return p => true;
            }
        }

        public Func<FieldInfo, bool> ShouldMapField
        {
            get
            {
                return p => p.IsPublic;
            }
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
            var mappingOptions = new Profile("Test");
            //mappingOptions.SourceMemberNamingConvention = new PascalCaseNamingConvention();
            //mappingOptions.DestinationMemberNamingConvention = new PascalCaseNamingConvention();

            var typeMap = _factory.CreateTypeMap(typeof(Source), typeof(Destination), mappingOptions, MemberList.Destination);

            var propertyMaps = typeMap.GetPropertyMaps();

            propertyMaps.Count().ShouldEqual(2);
        }
    }

    public class When_using_a_custom_source_naming_convention : SpecBase
    {
        private TypeMapFactory _factory;
        private TypeMap _map;
        private IProfileConfiguration _mappingOptions;
        
        private class Source
        {
            public SubSource some__source { get; set; }
        }

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
            var namingConvention = new StubNamingConvention(s => s.Value.ToLower()){SeparatorCharacter = "__", SplittingExpression = new Regex(@"[\p{Ll}\p{Lu}0-9]+(?=__?)")};

            _mappingOptions = new Profile("Test");
            _mappingOptions.AddMemberConfiguration().AddMember<NameSplitMember>(_ =>
            {
                _.SourceMemberNamingConvention = namingConvention;
                _.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
            });

            _factory = new TypeMapFactory();

        }

        protected override void Because_of()
        {
            _map = _factory.CreateTypeMap(typeof(Source), typeof(Destination), _mappingOptions, MemberList.Destination);
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
        private IProfileConfiguration _mappingOptions;

        private class Source
        {
            public SubSource SomeSource { get; set; }
        }

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
            var namingConvention = new StubNamingConvention(s => s.Value.ToLower()) { SeparatorCharacter = "__", SplittingExpression = new Regex(@"[\p{Ll}\p{Lu}0-9]+(?=__?)") };

            _mappingOptions = new Profile("Test");
            _mappingOptions.AddMemberConfiguration().AddMember<NameSplitMember>(_ =>
            {
                _.SourceMemberNamingConvention = new PascalCaseNamingConvention();
                _.DestinationMemberNamingConvention = namingConvention;
            });

            _factory = new TypeMapFactory();
        }

        protected override void Because_of()
        {
            _map = _factory.CreateTypeMap(typeof(Source), typeof(Destination), _mappingOptions, MemberList.Destination);
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
            public int �v�ator { get; set; }
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
            Mapper.Configuration.ReplaceMemberName("A", "�");
            Mapper.Configuration.ReplaceMemberName("i", "�");
            Mapper.Configuration.ReplaceMemberName("Airline", "Airlina");
            
            Mapper.CreateMap<Source,Destination>();

            var dest = Mapper.Map<Destination>(new Source {�v�ator = 3, SubAirlinaFlight = 4, Value = 5});
            dest.Aviator.ShouldEqual(3);
            dest.SubAirlineFlight.ShouldEqual(4);
            dest.Value.ShouldEqual(5);
        }
    }
}
