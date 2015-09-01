using System;
using System.Linq.Expressions;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    public class When_configuring__non_generic_ctor_param_members : AutoMapperSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest(int thing)
            {
                Value1 = thing;
            }

            public int Value1 { get; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap(typeof(Source), typeof(Dest)).ForCtorParam("thing", opt => opt.MapFrom(src => ((Source)src).Value));
            });
        }

        [Fact]
        public void Should_redirect_value()
        {
            var dest = Mapper.Map<Source, Dest>(new Source { Value = 5 });

            dest.Value1.ShouldEqual(5);
        }
    }
}