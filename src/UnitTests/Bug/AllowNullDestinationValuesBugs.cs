using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    namespace AllowNullDestinationValuesBugs
    {
        public class When_mapping_to_an_assignable_object_with_nullable_off : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public Inner Property { get; set; }
            }

            public class Destination
            {
                public Inner SomeOtherProperty { get; set; }
            }

            public class Inner
            {
                public string Name { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(config =>
            {
                config.AllowNullDestinationValues = false;
                config.CreateMap<Inner, Inner>();
                config.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.SomeOtherProperty, opt => opt.MapFrom(src => src.Property));
            });

            protected override void Because_of()
            {
                Source source = new Source();

                _destination = Mapper.Map<Source, Destination>(source);
            }

            [Fact]
            public void Should_create_the_assingable_member()
            {
                _destination.ShouldNotBeNull();
                _destination.SomeOtherProperty.ShouldNotBeNull();
            }
        }
    }
}