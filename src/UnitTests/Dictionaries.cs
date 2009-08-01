using System.Collections;
using System.Collections.Generic;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace Dictionaries
	{
		public class When_mapping_to_a_non_generic_dictionary : AutoMapperSpecBase
		{
			private Destination _result;

			private class Source
			{
				public Hashtable Values { get; set; }
			}

			private class Destination
			{
				public IDictionary Values { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				var source = new Source
					{
						Values = new Hashtable
							{
								{"Key1", "Value1"},
								{"Key2", 4}
							}
					};

				_result = Mapper.Map<Source, Destination>(source);
			}

			[Test]
			public void Should_map_the_source_dictionary_with_all_keys_and_values_preserved()
			{
				_result.Values.Count.ShouldEqual(2);

				_result.Values["Key1"].ShouldEqual("Value1");
				_result.Values["Key2"].ShouldEqual(4);
			}
		}

		public class When_mapping_to_a_generic_dictionary_with_mapped_value_pairs : SpecBase
		{
			private Destination _result;

			private class Source
			{
				public Dictionary<string, SourceValue> Values { get; set; }
			}

			private class SourceValue
			{
				public int Value { get; set; }
			}

			private class Destination
			{
				public Dictionary<string, DestinationValue> Values { get; set; }
			}

			private class DestinationValue
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
				Mapper.CreateMap<SourceValue, DestinationValue>();
			}

			protected override void Because_of()
			{
				var source = new Source
					{
						Values = new Dictionary<string, SourceValue>
							{
								{"Key1", new SourceValue {Value = 5}},
								{"Key2", new SourceValue {Value = 10}},
							}
					};

				_result = Mapper.Map<Source, Destination>(source);
			}

			[Test]
			public void Should_perform_mapping_for_individual_values()
			{
				_result.Values.Count.ShouldEqual(2);

				_result.Values["Key1"].Value.ShouldEqual(5);
				_result.Values["Key2"].Value.ShouldEqual(10);
			}
		}

		public class When_mapping_to_a_generic_dictionary_interface_with_mapped_value_pairs : SpecBase
		{
			private Destination _result;

			private class Source
			{
				public Dictionary<string, SourceValue> Values { get; set; }
			}

			private class SourceValue
			{
				public int Value { get; set; }
			}

			private class Destination
			{
				public IDictionary<string, DestinationValue> Values { get; set; }
			}

			private class DestinationValue
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
				Mapper.CreateMap<SourceValue, DestinationValue>();
			}

			protected override void Because_of()
			{
				var source = new Source
					{
						Values = new Dictionary<string, SourceValue>
							{
								{"Key1", new SourceValue {Value = 5}},
								{"Key2", new SourceValue {Value = 10}},
							}
					};

				_result = Mapper.Map<Source, Destination>(source);
			}

			[Test]
			public void Should_perform_mapping_for_individual_values()
			{
				_result.Values.Count.ShouldEqual(2);

				_result.Values["Key1"].Value.ShouldEqual(5);
				_result.Values["Key2"].Value.ShouldEqual(10);
			}
		}

		public class When_mapping_from_a_source_with_a_null_dictionary_member : AutoMapperSpecBase
		{
			private FooDto _result;

			public class Foo
			{
				public IDictionary<string, Foo> Bar { get; set; }
			}

			public class FooDto
			{
				public IDictionary<string, FooDto> Bar { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.AllowNullDestinationValues = false;
				Mapper.CreateMap<Foo, FooDto>();
				Mapper.CreateMap<FooDto, Foo>();
			}

			protected override void Because_of()
			{
				var foo1 = new Foo
				{
					Bar = new Dictionary<string, Foo>
							{
								{"lol", new Foo()}
							}
				};

				_result = Mapper.Map<Foo, FooDto>(foo1);
			}

			[Test]
			public void Should_fill_the_destination_with_an_empty_dictionary()
			{
				_result.Bar["lol"].Bar.ShouldNotBeNull();
				_result.Bar["lol"].Bar.ShouldBeInstanceOf<Dictionary<string, FooDto>>();
			}
		}
	}
}