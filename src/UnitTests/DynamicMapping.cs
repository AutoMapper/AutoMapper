using System;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	namespace DynamicMapping
	{
		public class When_mapping_two_non_configured_types : AutoMapperSpecBase
		{
			private Destination _resultWithGenerics;
			private Destination _resultWithoutGenerics;

			private class Source
			{
				public int Value { get; set; }
			}

			private class Destination
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

		public class When_mapping_two_non_configured_types_that_do_not_match : AutoMapperSpecBase
		{
			private Exception _thrown;

			private class Source
			{
				public int Value { get; set; }
			}

			private class Destination
			{
				public int Valuefff { get; set; }
			}

			protected override void Because_of()
			{
				try
				{
					Mapper.DynamicMap<Source, Destination>(new Source { Value = 5 });
				}
				catch (AutoMapperConfigurationException ex)
				{
					_thrown = ex;
				}
			}

			[Test]
			public void Should_validate_the_configuration_attempted()
			{
				_thrown.ShouldNotBeNull();
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