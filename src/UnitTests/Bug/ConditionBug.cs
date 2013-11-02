namespace AutoMapper.UnitTests.Bug
{
    namespace ConditionBug
    {
        using System.Collections.Generic;
        using Should;
        using Xunit;

        public class Example : AutoMapperSpecBase
        {
            public class SubSource
            {
                public string SubValue { get; set; }
            }

            public class Source
            {
                public Source()
                {
                    Value = new List<SubSource>();
                }

                public List<SubSource> Value { get; set; }
            }

            public class Destination
            {
                public string Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt =>
                    {
                        opt.Condition(src => src.Value.Count > 1);
                        opt.ResolveUsing(src => src.Value[1].SubValue);
                    }));
            }

            [Fact]
            public void Should_skip_the_mapping_when_the_condition_is_false()
            {
                var src = new Source();
                src.Value.Add(new SubSource {SubValue = "x"});
                var destination = Mapper.Map<Source, Destination>(src);

                destination.Value.ShouldBeNull();
            }

            [Fact]
            public void Should_execute_the_mapping_when_the_condition_is_true()
            {
                var src = new Source();
                src.Value.Add(new SubSource {SubValue = "x"});
                src.Value.Add(new SubSource {SubValue = "x"});
                var destination = Mapper.Map<Source, Destination>(src);

                destination.Value.ShouldEqual("x");
            }
        }

        public class PrimitiveExample : AutoMapperSpecBase
        {
            public class Source
            {
                public int? Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Source, Destination>()
                    .ForMember(d => d.Value, opt =>
                    {
                        opt.Condition(src => src.Value.HasValue);
                        opt.MapFrom(src => src.Value.Value + 10);
                    }));
            }


            [Fact]
            public void Should_skip_when_condition_not_met()
            {
                var dest = Mapper.Map<Source, Destination>(new Source());

                dest.Value.ShouldEqual(0);
            }
        }
    }
}