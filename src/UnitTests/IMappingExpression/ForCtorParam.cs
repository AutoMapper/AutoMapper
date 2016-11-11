using System;
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

        public class DestWithNoConstructor
        {
            public int Value1 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Source), typeof(Dest))
                .ForCtorParam("thing", opt => opt.MapFrom(src => ((Source)src).Value));
        });

        [Fact]
        public void Should_redirect_value()
        {
            var dest = Mapper.Map<Source, Dest>(new Source { Value = 5 });

            dest.Value1.ShouldEqual(5);
        }

        [Fact]
        public void Should_resolve_using_custom_func()
        {
            var mapper = new MapperConfiguration(
                cfg => cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt => opt.ResolveUsing(src =>
                {
                    var rev = src.Value + 3;
                    return rev;
                })))
                .CreateMapper();

            var dest = mapper.Map<Source, Dest>(new Source { Value = 5 });

            dest.Value1.ShouldEqual(8);
        }

        [Fact]
        public void Should_resolve_using_custom_func_with_correct_ResolutionContext()
        {
            const string itemKey = "key";
            var mapper = new MapperConfiguration(
                cfg => cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt =>
                    opt.ResolveUsing((src, ctx) => ctx.Items[itemKey])
                ))
                .CreateMapper();

            var dest = mapper.Map<Source, Dest>(new Source { Value = 8 },
                opts => opts.Items[itemKey] = 10);

            dest.Value1.ShouldEqual(10);
        }

        [Fact]
        public void Should_throw_on_nonexistent_parameter()
        {
            Action configuration = () => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForCtorParam("thing", opt => opt.MapFrom(src => src.Value))
                    .ForCtorParam("think", opt => opt.MapFrom(src => src.Value));
            });
            configuration.ShouldThrow<AutoMapperConfigurationException>(exception =>
            {
                exception.Message.ShouldContain("does not have a constructor with a parameter named 'think'.", StringComparison.InvariantCulture);
                exception.Message.ShouldContain(typeof(Dest).FullName, StringComparison.InvariantCulture);
            });
        }

        [Fact]
        public void Should_throw_when_no_constructor_is_present()
        {
            Action configuration = () => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, DestWithNoConstructor>()
                    .ForMember(dest => dest.Value1, opt => opt.MapFrom(src => src.Value))
                    .ForCtorParam("thing", opt => opt.MapFrom(src => src.Value));
            });

            configuration.ShouldThrow<AutoMapperConfigurationException>(exception =>
            {
                exception.Message.ShouldContain("does not have a constructor.", StringComparison.InvariantCulture);
                exception.Message.ShouldContain(typeof(Dest).FullName, StringComparison.InvariantCulture);
            });
        }

        [Fact]
        public void Should_throw_when_parameter_is_misspelt()
        {
            Action configuration = () => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForCtorParam("think", opt => opt.MapFrom(src => src.Value));
            });

            configuration.ShouldThrow<AutoMapperConfigurationException>(exception =>
            {
                exception.Message.ShouldContain("does not have a constructor with a parameter named 'think'.", StringComparison.InvariantCulture);
                exception.Message.ShouldContain(typeof(Dest).FullName, StringComparison.InvariantCulture);
            });
        }
    }
}