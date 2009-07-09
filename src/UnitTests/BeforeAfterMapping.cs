using NBehave.Spec.NUnit;
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
}
