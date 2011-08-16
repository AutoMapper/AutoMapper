using System;
using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests
{
	namespace DynamicMapping
	{
		public class When_mapping_two_non_configured_types : AutoMapperSpecBase
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

			protected override void Because_of()
			{
				_resultWithGenerics = Mapper.DynamicMap<Source, Destination>(new Source {Value = 5});
				_resultWithoutGenerics = (Destination) Mapper.DynamicMap(new Source {Value = 5}, typeof(Source), typeof(Destination));
			}

			[Test]
			public void Should_dynamically_map_the_two_types()
			{
				_resultWithGenerics.Value.ShouldEqual(5);
				_resultWithoutGenerics.Value.ShouldEqual(5);
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

			[Test]
			public void Should_ignore_any_members_that_do_not_match()
			{
				var destination = Mapper.DynamicMap<Source, Destination>(new Source {Value = 5});

				destination.Valuefff.ShouldEqual(0);
			}

			[Test]
			public void Should_not_throw_any_configuration_errors()
			{
				typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.DynamicMap<Source, Destination>(new Source { Value = 5 }));
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

			protected override void Because_of()
			{
				_destination = new Destination { Valuefff = 7};
				Mapper.DynamicMap(new Source { Value = 5, Value2 = 3}, _destination);
			}

			[Test]
			public void Should_preserve_existing_values()
			{
				_destination.Valuefff.ShouldEqual(7);
			}

			[Test]
			public void Should_map_new_values()
			{
				_destination.Value2.ShouldEqual(3);
			}
		}

		public class When_mapping_from_an_anonymous_type_to_an_interface : SpecBase
		{
			private IDestination _result;

			public interface IDestination
			{
				int Value { get; set; }
			}

			protected override void Because_of()
			{
				_result = Mapper.DynamicMap<IDestination>(new {value = 5});
			}

			[Test]
			public void Should_allow_dynamic_mapping()
			{
				_result.Value.ShouldEqual(5);
			}
		}

	}
}