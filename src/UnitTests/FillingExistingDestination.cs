using System.Collections.Generic;
using Should;
using Xunit;
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

			[Fact]
			public void Should_do_the_translation()
			{
				_dest.Value.ShouldEqual(10);
			}

			[Fact]
			public void Should_return_the_destination_object_that_was_passed_in()
			{
				_originalDest.ShouldBeSameAs(_dest);
			}
		}
       
        public class When_the_destination_object_is_specified_with_child_objects : AutoMapperSpecBase
        {
            private Source _source;
            private Destination _originalDest;
            private Destination _dest;

            public class Source
            {
                public int Value { get; set; }
                public ChildSource Child { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
                public string Name { get; set; }
                public ChildDestination Child { get; set; }
            }

            public class ChildSource
            {
                public int Value { get; set; }
            }

            public class ChildDestination
            {
                public int Value { get; set; }
                public string Name { get; set; }
            }

            protected override void Establish_context()
            {
                base.Establish_context();

                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source);
                    cfg.CreateMap<ChildSource, ChildDestination>(MemberList.Source);
                });

                _source = new Source
                {
                    Value = 10,
                    Child = new ChildSource
                    {
                        Value = 20
                    }
                };
            }

            protected override void Because_of()
            {
                _originalDest = new Destination
                {
                    Value = 1111,
                    Name = "foo",
                    Child = new ChildDestination
                    {
                        Name = "bar"
                    }
                };
                _dest = Mapper.Map<Source, Destination>(_source, _originalDest);
            }

            [Fact]
            public void Should_do_the_translation()
            {
                _dest.Value.ShouldEqual(10);
                _dest.Child.Value.ShouldEqual(20);
            }

            [Fact]
            public void Should_return_the_destination_object_that_was_passed_in()
            {
                _dest.Name.ShouldEqual("foo");
                _dest.Child.Name.ShouldEqual("bar");
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

			[Fact]
			public void Should_return_the_enum_as_a_string()
			{
				_result.ShouldEqual("Two");
			}
		}
	}
}