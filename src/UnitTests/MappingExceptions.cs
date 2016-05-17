using System;
using Xunit;
using Should;

namespace AutoMapper.UnitTests.MappingExceptions
{
    public class When_the_exception_does_not_have_the_types
    {
        [Fact]
        public void Should_provide_a_useful_message()
        {
            var ex = new AutoMapperMappingException();
            ex.Message.ShouldStartWith("Exception");
        }
    }

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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });

        [Fact]
        public void Should_provide_a_contextual_exception()
        {
            var source = new Source { Value = "adsf" };
            typeof(AutoMapperMappingException).ShouldBeThrownBy(() => Mapper.Map<Source, Dest>(source));
        }

        [Fact]
        public void Should_have_contextual_mapping_information()
        {
            var source = new Source { Value = "adsf" };
            AutoMapperMappingException thrown = null;
            try
            {
                Mapper.Map<Source, Dest>(source);
            }
            catch (AutoMapperMappingException ex)
            {
                thrown = ex;
            }
            thrown.ShouldNotBeNull();
        }
    }
}