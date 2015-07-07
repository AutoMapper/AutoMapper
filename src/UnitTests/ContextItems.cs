namespace AutoMapper.UnitTests
{
    namespace ContextItems
    {
        using Should;
        using Xunit;

        public class When_mapping_with_contextual_values : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
            }

            public class ContextResolver : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New((int) source.Value + (int)source.Context.Options.Items["Item"]);
                }
            }

            [Fact]
            public void Should_use_value_passed_in()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Dest>()
                        .ForMember(d => d.Value, opt => opt.ResolveUsing<ContextResolver>().FromMember(src => src.Value));
                });

                var dest = Mapper.Map<Source, Dest>(new Source { Value = 5 }, opt => { opt.Items["Item"] = 10; });

                dest.Value.ShouldEqual(15);
            }
        }

        public class When_mapping_with_contextual_values_in_resolve_func : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value1 { get; set; }
            }

            public class Dest
            {
                public int Value1 { get; set; }
            }

            public class ContextResolver : IValueResolver
            {
                public ResolutionResult Resolve(ResolutionResult source)
                {
                    return source.New((int) source.Value + (int)source.Context.Options.Items["Item"]);
                }
            }

            [Fact]
            public void Should_use_value_passed_in()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Dest>()
                        .ForMember(d => d.Value1, opt => opt.ResolveUsing(result => (int)result.Context.Options.Items["Item"] + ((Source)result.Value).Value1));
                });

                var dest = Mapper.Map<Source, Dest>(new Source { Value1 = 5 }, opt => { opt.Items["Item"] = 10; });

                dest.Value1.ShouldEqual(15);
            }
        }
    }
}