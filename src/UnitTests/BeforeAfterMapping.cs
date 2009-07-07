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
}
