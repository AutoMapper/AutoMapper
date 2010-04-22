using System;
using System.Text.RegularExpressions;
using NBehave.Spec.NUnit;
using NUnit.Framework;
using System.Linq;
using Rhino.Mocks;

namespace AutoMapper.UnitTests.Tests
{
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

		[Test]
        public void Should_map_properties_with_same_name()
        {
			var mappingOptions = CreateStub<IMappingOptions>();
			mappingOptions.SourceMemberNamingConvention = new PascalCaseNamingConvention();
			mappingOptions.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
            mappingOptions.SourceMemberNameTransformer = s => s;
            mappingOptions.DestinationMemberNameTransformer = s => s;
			
			var typeMap = _factory.CreateTypeMap(typeof(Source), typeof(Destination), mappingOptions);

            var propertyMaps = typeMap.GetPropertyMaps();
            
            propertyMaps.Count().ShouldEqual(2);
        }
    }

	public class When_using_a_custom_source_naming_convention : SpecBase
	{
		private TypeMapFactory _factory;
		private TypeMap _map;
		private IMappingOptions _mappingOptions;

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
			INamingConvention namingConvention = CreateStub<INamingConvention>();
			namingConvention.Stub(nc => nc.SeparatorCharacter).Return("__");

			_mappingOptions = CreateStub<IMappingOptions>();
			_mappingOptions.SourceMemberNamingConvention = namingConvention;
			_mappingOptions.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
			_mappingOptions.SourceMemberNameTransformer = s => s;
            _mappingOptions.DestinationMemberNameTransformer = s => s;

			_factory = new TypeMapFactory();

		}

		protected override void Because_of()
		{
			_map = _factory.CreateTypeMap(typeof(Source), typeof(Destination), _mappingOptions);
		}

		[Test]
		public void Should_split_using_naming_convention_rules()
		{
			_map.GetPropertyMaps().Count().ShouldEqual(1);
		}
	}

	public class When_using_a_custom_destination_naming_convention : SpecBase
	{
		private TypeMapFactory _factory;
		private TypeMap _map;
		private IMappingOptions _mappingOptions;

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
			INamingConvention namingConvention = CreateStub<INamingConvention>();

			namingConvention.Stub(nc => nc.SplittingExpression).Return(new Regex(@"[\p{Ll}0-9]*(?=_?)"));

			_mappingOptions = CreateStub<IMappingOptions>();
			_mappingOptions.SourceMemberNamingConvention = new PascalCaseNamingConvention();
			_mappingOptions.DestinationMemberNamingConvention = namingConvention;
			_mappingOptions.SourceMemberNameTransformer = s => s;
            _mappingOptions.DestinationMemberNameTransformer = s => s;

			_factory = new TypeMapFactory();
		}

		protected override void Because_of()
		{
			_map = _factory.CreateTypeMap(typeof(Source), typeof(Destination), _mappingOptions);
		}

		[Test]
		public void Should_split_using_naming_convention_rules()
		{
			_map.GetPropertyMaps().Count().ShouldEqual(1);
		}
	}

	public class When_specifying_a_custom_transformation_method : SpecBase
	{
		private IMappingOptions _mappingOptions;
		private TypeMapFactory _factory;
		private TypeMap _map;

		private class Source
		{
			public int FooValueBar { get; set; }
			public int FooValueBar2 { get; set; }
		}

		private class Destination
		{
			public int Value { get; set; }
			public int FooValueBar2 { get; set; }
		}

		protected override void Establish_context()
		{
			_mappingOptions = CreateStub<IMappingOptions>();
			Func<string, string> transformer = s =>
				{
					s = Regex.Replace(s, "(?:^Foo)?(.*)", "$1");
					return Regex.Replace(s, "(.*)(?:Bar|Blah)$", "$1");
				};
			_mappingOptions.SourceMemberNameTransformer = transformer;
            _mappingOptions.DestinationMemberNameTransformer = s => s;
            _mappingOptions.SourceMemberNamingConvention = new PascalCaseNamingConvention();
			_mappingOptions.DestinationMemberNamingConvention = new PascalCaseNamingConvention();

			_factory = new TypeMapFactory();
		}

		protected override void Because_of()
		{
			_map = _factory.CreateTypeMap(typeof(Source), typeof(Destination), _mappingOptions);
		}

		[Test]
		public void Should_execute_transform_when_members_do_not_match()
		{
			_map.GetPropertyMaps().Count().ShouldEqual(2);
		}
	}

    public class When_specifying_a_custom_destination_transformation_method : SpecBase
    {
        private IMappingOptions _mappingOptions;
        private TypeMapFactory _factory;
        private TypeMap _map;

        private class Source
        {
            public int Value { get; set; }
            public int FooValueBar2 { get; set; }
        }

        private class Destination
        {
            public int FooValueBar { get; set; }
            public int FooValueBar2 { get; set; }
        }

        protected override void Establish_context()
        {
            _mappingOptions = CreateStub<IMappingOptions>();
            Func<string, string> transformer = s =>
            {
                s = Regex.Replace(s, "(?:^Foo)?(.*)", "$1");
                return Regex.Replace(s, "(.*)(?:Bar|Blah)$", "$1");
            };
            _mappingOptions.SourceMemberNameTransformer = s => s;
            _mappingOptions.DestinationMemberNameTransformer = transformer;
            _mappingOptions.SourceMemberNamingConvention = new PascalCaseNamingConvention();
            _mappingOptions.DestinationMemberNamingConvention = new PascalCaseNamingConvention();

            _factory = new TypeMapFactory();
        }

        protected override void Because_of()
        {
            _map = _factory.CreateTypeMap(typeof(Source), typeof(Destination), _mappingOptions);
        }

        [Test]
        public void Should_execute_transform_when_members_do_not_match()
        {
            _map.GetPropertyMaps().Count().ShouldEqual(2);
        }
    }
}