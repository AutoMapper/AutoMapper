using Xunit;

namespace AutoMapper.UnitTests
{
    public class ConfigurationRules : SpecBase
    {
        public class Source { }
        public class Dest { }

        public class Profile1 : Profile
        {
            public Profile1()
            {
                CreateMap<Source, Dest>();
            }
        }

        public class Profile2 : Profile
        {
            public Profile2()
            {
                CreateMap<Source, Dest>();
            }
        }

        [Fact]
        public void Should_throw_for_multiple_create_map_calls()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
                cfg.CreateMap<Source, Dest>();
            });

            typeof(DuplicateTypeMapConfigurationException).ShouldBeThrownBy(() => config.AssertConfigurationIsValid());
        }

        [Fact]
        public void Should_not_throw_when_allowing_multiple_create_map_calls()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
                cfg.CreateMap<Source, Dest>();
                cfg.Advanced.AllowAdditiveTypeMapCreation = true;
            });

            typeof(DuplicateTypeMapConfigurationException).ShouldNotBeThrownBy(() => config.AssertConfigurationIsValid());
        }

        [Fact]
        public void Should_throw_for_multiple_create_map_calls_in_different_profiles()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Profile1>();
                cfg.AddProfile<Profile2>();
            });

            typeof(DuplicateTypeMapConfigurationException).ShouldBeThrownBy(() => config.AssertConfigurationIsValid());
        }

        [Fact]
        public void Should_not_throw_when_allowing_multiple_create_map_calls_in_different_profiles()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Profile1>();
                cfg.AddProfile<Profile2>();
                cfg.Advanced.AllowAdditiveTypeMapCreation = true;
            });

            typeof(DuplicateTypeMapConfigurationException).ShouldNotBeThrownBy(() => config.AssertConfigurationIsValid());
        }
    }
}