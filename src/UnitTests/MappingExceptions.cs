namespace AutoMapper.UnitTests
{
    using Should;
    using Xunit;

    namespace MappingExceptions
    {
        public class When_encountering_a_member_mapping_problem_during_mapping : NonValidatingSpecBase
        {
            public class Source
            {
                public string Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<Source, Dest>();
            }

            [Fact]
            public void Should_provide_a_contextual_exception()
            {
                var source = new Source {Value = "adsf"};
                typeof (AutoMapperMappingException).ShouldBeThrownBy(() => Mapper.Map<Source, Dest>(source));
            }

            [Fact]
            public void Should_have_contextual_mapping_information()
            {
                var source = new Source {Value = "adsf"};
                AutoMapperMappingException thrownException = null;
                try
                {
                    Mapper.Map<Source, Dest>(source);
                }
                catch (AutoMapperMappingException ex)
                {
                    thrownException = ex;
                }
                thrownException.ShouldNotBeNull();
            }
        }
    }
}