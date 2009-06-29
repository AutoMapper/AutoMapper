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

			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }

                public int OtherValue { get; set; }
			}

			protected override void Establish_context()
			{
				base.Establish_context();

				Mapper.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.OtherValue, opt => opt.Ignore());

				_source = new Source
				{
					Value = 10,
				};
			}

			protected override void Because_of()
			{
				_originalDest = new Destination { Value = 1111, OtherValue = 1234};
				Mapper.Map(_source, _originalDest);
			}

			[Test]
			public void Should_do_the_translation()
			{
                _originalDest.Value.ShouldEqual(10);
			}

			[Test]
			public void Should_return_the_destination_object_that_was_passed_in()
			{
				_originalDest.OtherValue.ShouldEqual(1234);
			}
		}
	}
}