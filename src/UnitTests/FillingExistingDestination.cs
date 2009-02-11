using System.Collections.Generic;
using NBehave.Spec.NUnit;
using NUnit.Framework;
using System.Linq;

namespace AutoMapper.UnitTests
{
	namespace FillingExistingDestination
	{

		public class When_the_destination_object_is_specified : AutoMapperSpecBase
		{
			private Source _source;
			private Destination _originalDest;
			private Destination _dest;

			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				base.Establish_context();

				Mapper.CreateMap<Source, Destination>();

				_source = new Source
				{
					Value = 10,
				};
			}

			protected override void Because_of()
			{
				_originalDest = new Destination { Value = 1111 };
				_dest = Mapper.Map<Source, Destination>(_source, _originalDest);
			}

			[Test]
			public void Should_do_the_translation()
			{
				_dest.Value.ShouldEqual(10);
			}

			[Test]
			public void Should_return_the_destination_object_that_was_passed_in()
			{
				_originalDest.ShouldBeTheSameAs(_dest);
			}
		}

		public class When_the_destination_array_is_specified : AutoMapperSpecBase
		{
			private List<Source> _sourceList;
			private Destination[] _destinationList;

			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }
			}

			public class DestinationList : List<Destination>
			{
				public string Name { get; set; }
			}

			protected override void Establish_context()
			{
				base.Establish_context();

				Mapper.CreateMap<Source, Destination>();

				_sourceList = new List<Source>
					{
						new Source() {Value = 10},
						new Source() {Value = 11},
						new Source() {Value = 12},
					};
				_destinationList = new DestinationList() { new Destination() { Value = 100 } }.ToArray();
			}

			protected override void Because_of()
			{
				_destinationList = Mapper.Map<IList<Source>, Destination[]>(_sourceList, _destinationList);
			}

			[Test]
			public void Should_ignore_the_array_passed_in()
			{
				_destinationList.Any(d => d.Value == 100).ShouldBeFalse();
			}

			[Test]
			public void Should_translate_the_elements_in_the_list()
			{
				_destinationList.Count().ShouldEqual(3);
				_destinationList.Any(d => d.Value == 10).ShouldBeTrue();
				_destinationList.Any(d => d.Value == 11).ShouldBeTrue();
				_destinationList.Any(d => d.Value == 12).ShouldBeTrue();
			}
		}

		public class When_the_destination_object_is_specified_and_you_are_converting_an_enum : AutoMapperSpecBase
		{
			private string _result;

			public enum SomeEnum
			{
				One,
				Two,
				Three
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<SomeEnum, string>(SomeEnum.Two, "test");
			}

			[Test]
			public void Should_return_the_enum_as_a_string()
			{
				_result.ShouldEqual("Two");
			}
		}
	}
}