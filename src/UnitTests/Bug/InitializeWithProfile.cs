using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class InitializeWithProfile : AutoMapperSpecBase
    {
        const int SomeValue = 8766;
        private Destination _destination;

        class Source
        {
            public int Number { get; set; }
        }
        class Destination
        {
            public int Number { get; set; }
        }

        class MyProfile : Profile
        {
            protected override void Configure()
            {
                CreateMap<Source, Destination>();
            }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize<MyProfile>();
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Number = SomeValue });
        }

        [Fact]
        public void Should_initalize_with_profile()
        {
            _destination.Number.ShouldEqual(SomeValue);
        }
    }
}