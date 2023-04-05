namespace AutoMapper.UnitTests;

public class ConfigurationRules : NonValidatingSpecBase
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
    public void Should_throw_for_multiple_create_map_calls_in_configuration_expression_and_profile()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
            cfg.AddProfile<Profile1>();
        });

        new Action(() => config.AssertConfigurationIsValid()).ShouldThrowException<DuplicateTypeMapConfigurationException>(c =>
        {
            c.Errors.SelectMany(t => t.ProfileNames).ShouldNotContain(string.Empty);
        });
    }

}