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
        public void Should_ignore_nonexistent_parameter()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForCtorParam("thing", opt => opt.MapFrom(src => src.Value))
                    .ForCtorParam("think", opt => opt.MapFrom(src => src.Value));
            });

            config.AssertConfigurationIsValid();

            var result = config.CreateMapper().Map<Dest>(new Source { Value = 42 });

            result.Value1.ShouldEqual(42);
        }

        [Fact]
        public void Should_ignore_when_no_constructor_is_present()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, DestWithNoConstructor>()
                    .ForMember(dest => dest.Value1, opt => opt.MapFrom(src => src.Value))
                    .ForCtorParam("thing", opt => opt.MapFrom(src => src.Value));
            });

            config.AssertConfigurationIsValid();

            var result = config.CreateMapper().Map<DestWithNoConstructor>(new Source { Value = 17 });

            result.Value1.ShouldEqual(17);
        }

        [Fact]
        public void Should_not_pass_config_validation_when_parameter_is_mispelt()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForCtorParam("think", opt => opt.MapFrom(src => src.Value));
            });

            Action configValidation = () => config.AssertConfigurationIsValid();
            configValidation.ShouldThrow<AutoMapperConfigurationException>();
        }
    }
}