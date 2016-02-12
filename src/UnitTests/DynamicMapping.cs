using System;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
	namespace DynamicMapping
	{
		public class When_mapping_two_non_configured_types : NonValidatingSpecBase
		{
			private Destination _resultWithGenerics;
			private Destination _resultWithoutGenerics;

			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }
			}

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

            [Fact]
			public void Should_dynamically_map_the_two_types()
			{
				_resultWithGenerics = Mapper.Map<Source, Destination>(new Source {Value = 5});
				_resultWithoutGenerics = (Destination) Mapper.Map(new Source {Value = 5}, typeof(Source), typeof(Destination));
				_resultWithGenerics.Value.ShouldEqual(5);
				_resultWithoutGenerics.Value.ShouldEqual(5);
			}
		}

        public class When_mapping_two_non_configured_types_with_nesting : NonValidatingSpecBase
        {
            private Destination _resultWithGenerics;

            public class Source
            {
                public int Value { get; set; }
                public ChildSource Child { get; set; }
            }

            public class ChildSource
            {
                public string Value2 { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
                public ChildDestination Child { get; set; }
            }

            public class ChildDestination
            {
                public string Value2 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

            public When_mapping_two_non_configured_types_with_nesting()
            {
                var source = new Source
                {
                    Value = 5,
                    Child = new ChildSource
                    {
                        Value2 = "foo"
                    }
                };
                _resultWithGenerics = Mapper.Map<Source, Destination>(source);
            }

            [Fact]
            public void Should_dynamically_map_the_two_types()
            {
                _resultWithGenerics.Value.ShouldEqual(5);
            }

            [Fact]
            public void Should_dynamically_map_the_children()
            {
                _resultWithGenerics.Child.Value2.ShouldEqual("foo");
            }
        }

		public class When_mapping_two_non_configured_types_that_do_not_match : NonValidatingSpecBase
		{
			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Valuefff { get; set; }
			}

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

            [Fact]
			public void Should_ignore_any_members_that_do_not_match()
			{
				var destination = Mapper.Map<Source, Destination>(new Source {Value = 5});

				destination.Valuefff.ShouldEqual(0);
			}

			[Fact]
			public void Should_not_throw_any_configuration_errors()
			{
				typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.Map<Source, Destination>(new Source { Value = 5 }));
			}
		}

		public class When_mapping_to_an_existing_destination_object : NonValidatingSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int Value { get; set; }
				public int Value2 { get; set; }
			}

			public class Destination
			{
				public int Valuefff { get; set; }
				public int Value2 { get; set; }
			}

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

            public When_mapping_to_an_existing_destination_object()
		    {
				_destination = new Destination { Valuefff = 7};
				Mapper.Map(new Source { Value = 5, Value2 = 3}, _destination);
			}

			[Fact]
			public void Should_preserve_existing_values()
			{
				_destination.Valuefff.ShouldEqual(7);
			}

			[Fact]
			public void Should_map_new_values()
			{
				_destination.Value2.ShouldEqual(3);
			}
		}

#if !PORTABLE
        public class When_mapping_from_an_anonymous_type_to_an_interface : NonValidatingSpecBase
		{
			private IDestination _result;

			public interface IDestination
			{
				int Value { get; set; }
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<IDestination>(new {Value = 5});
			}

			[Fact]
			public void Should_allow_dynamic_mapping()
			{
				_result.Value.ShouldEqual(5);
			}

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
		}
#endif
    }
}