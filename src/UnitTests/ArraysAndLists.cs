using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	namespace ArraysAndLists
	{
		public class When_mapping_to_a_concrete_non_generic_ienumerable : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int[] Values { get; set; }
				public List<int> Values2 { get; set; }
			}

			public class Destination
			{
				public IEnumerable Values { get; set; }
				public IEnumerable Values2 { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}, Values2 = new List<int> {9, 8, 7, 6}});
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain(1);
				_destination.Values.ShouldContain(2);
				_destination.Values.ShouldContain(3);
				_destination.Values.ShouldContain(4);
			}

			[Test]
			public void Should_map_from_the_generic_list_of_values()
			{
				_destination.Values2.ShouldNotBeNull();
				_destination.Values2.ShouldContain(9);
				_destination.Values2.ShouldContain(8);
				_destination.Values2.ShouldContain(7);
				_destination.Values2.ShouldContain(6);
			}
		}

		public class When_mapping_to_a_concrete_generic_ienumerable : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int[] Values { get; set; }
				public List<int> Values2 { get; set; }
			}

			public class Destination
			{
				public IEnumerable<int> Values { get; set; }
				public IEnumerable<string> Values2 { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain(1);
				_destination.Values.ShouldContain(2);
				_destination.Values.ShouldContain(3);
				_destination.Values.ShouldContain(4);
			}

			[Test]
			public void Should_map_from_the_generic_list_of_values_with_formatting()
			{
				_destination.Values2.ShouldNotBeNull();
				_destination.Values2.ShouldContain("9");
				_destination.Values2.ShouldContain("8");
				_destination.Values2.ShouldContain("7");
				_destination.Values2.ShouldContain("6");
			}
		}

		public class When_mapping_to_a_concrete_non_generic_icollection : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int[] Values { get; set; }
				public List<int> Values2 { get; set; }
			}

			public class Destination
			{
				public ICollection Values { get; set; }
				public ICollection Values2 { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}, Values2 = new List<int> {9, 8, 7, 6}});
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain(1);
				_destination.Values.ShouldContain(2);
				_destination.Values.ShouldContain(3);
				_destination.Values.ShouldContain(4);
			}

			[Test]
			public void Should_map_from_a_non_array_source()
			{
				_destination.Values2.ShouldNotBeNull();
				_destination.Values2.ShouldContain(9);
				_destination.Values2.ShouldContain(8);
				_destination.Values2.ShouldContain(7);
				_destination.Values2.ShouldContain(6);
			}
		}

		public class When_mapping_to_a_concrete_generic_icollection : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int[] Values { get; set; }
			}

			public class Destination
			{
				public ICollection<string> Values { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}});
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain("1");
				_destination.Values.ShouldContain("2");
				_destination.Values.ShouldContain("3");
				_destination.Values.ShouldContain("4");
			}
		}

		public class When_mapping_to_a_concrete_ilist : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int[] Values { get; set; }
			}

			public class Destination
			{
				public IList Values { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}});
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain(1);
				_destination.Values.ShouldContain(2);
				_destination.Values.ShouldContain(3);
				_destination.Values.ShouldContain(4);
			}
		}

		public class When_mapping_to_a_concrete_generic_ilist : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int[] Values { get; set; }
			}

			public class Destination
			{
				public IList<string> Values { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}});
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain("1");
				_destination.Values.ShouldContain("2");
				_destination.Values.ShouldContain("3");
				_destination.Values.ShouldContain("4");
			}
		}
	}
}