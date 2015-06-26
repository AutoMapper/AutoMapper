using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class AfterMapNestedObjects : AutoMapperSpecBase
    {
        bool _afterMapCalled;

        public class Inner
        {
            public string Prop1 { get; set; }
        }

        public class Outer
        {
            public Inner Inner { get; set; }
        }

        public class InnerDTO
        {
            public string Prop1 { get; set; }
        }

        public class OuterDTO
        {
            public InnerDTO Inner { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Inner, InnerDTO>();
            Mapper.CreateMap<Outer, OuterDTO>();
        }

        protected override void Because_of()
        {
            var outer = new Outer { Inner = new Inner() { Prop1 = "Prop1" } };
            Mapper.Map<Outer, OuterDTO>(outer, o => o.AfterMap((s, d) => _afterMapCalled = true));
        }

        [Fact]
        public void Should_call_aftermap_for_outer_objects()
        {
            _afterMapCalled.ShouldBeTrue();
        }
    }
}