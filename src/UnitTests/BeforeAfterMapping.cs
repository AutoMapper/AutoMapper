using System;
using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests.BeforeAfterMapping
{
    public class When_configuring_before_and_after_methods : AutoMapperSpecBase
    {
        private Source _src;

        public class Source
        {
        }
        public class Destination
        {
        }

        protected override void Establish_context()
        {
            _src = new Source();
        }

        [Test]
        public void Before_and_After_should_be_called()
        {
            var beforeMapCalled = false;
            var afterMapCalled = false;

            Mapper.CreateMap<Source, Destination>()
                .BeforeMap((src, dest) => beforeMapCalled = true)
                .AfterMap((src, dest) => afterMapCalled = true);

            Mapper.Map<Source, Destination>(_src);

            beforeMapCalled.ShouldBeTrue();
            afterMapCalled.ShouldBeTrue();
        }

    }

    public class When_configuring_before_and_after_methods_multiple_times : AutoMapperSpecBase
    {
        private Source _src;

        public class Source
        {
        }
        public class Destination
        {
        }

        protected override void Establish_context()
        {
            _src = new Source();
        }

        [Test]
        public void Before_and_After_should_be_called()
        {
            var beforeMapCount = 0;
            var afterMapCount = 0;

            Mapper.CreateMap<Source, Destination>()
                .BeforeMap((src, dest) => beforeMapCount++)
                .BeforeMap((src, dest) => beforeMapCount++)
                .AfterMap((src, dest) => afterMapCount++)
                .AfterMap((src, dest) => afterMapCount++);

            Mapper.Map<Source, Destination>(_src);

            beforeMapCount.ShouldEqual(2);
            afterMapCount.ShouldEqual(2);
        }

    }

	public class When_using_a_class_to_do_before_after_mappings : AutoMapperSpecBase
	{
		private Destination _destination;

		public class Source
		{
			public int Value { get; set; }
		}

		public class Destination
		{
			public int Value { get; set; }
		}

		public class BeforeMapAction : IMappingAction<Source, Destination>
		{
			private readonly int _decrement;

			public BeforeMapAction(int decrement)
			{
				_decrement = decrement;
			}

			public void Process(Source source, Destination destination)
			{
				source.Value -= _decrement * 2;
			}
		}

		public class AfterMapAction : IMappingAction<Source, Destination>
		{
			private readonly int _increment;

			public AfterMapAction(int increment)
			{
				_increment = increment;
			}

			public void Process(Source source, Destination destination)
			{
				destination.Value += _increment * 5;
			}
		}

		protected override void Establish_context()
		{
			Mapper.Initialize(i => i.ConstructServicesUsing(t => Activator.CreateInstance(t, 2)));

			Mapper.CreateMap<Source, Destination>()
				.BeforeMap<BeforeMapAction>()
				.AfterMap<AfterMapAction>();
		}

		protected override void Because_of()
		{
			_destination = Mapper.Map<Source, Destination>(new Source {Value = 4});
		}

		[Test]
		public void Should_use_global_constructor_for_building_mapping_actions()
		{
			_destination.Value.ShouldEqual(10);
		}
	}

}
