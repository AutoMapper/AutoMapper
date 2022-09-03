namespace AutoMapper.UnitTests.Bug;

public class IgnoreShouldBeInheritedIfConventionCannotMap
{
    public class BaseDomain
    {

    }

    public class StandardDomain : BaseDomain
    {
        
    }

    public class SpecificDomain : StandardDomain
    {
    }

    public class MoreSpecificDomain : SpecificDomain
    {
        
    }

    public class Dto
    {
        public string SpecificProperty { get; set; }
    }

    [Fact]
    public void inhertited_ignore_should_be_overridden_passes_validation()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseDomain, Dto>()
                .ForMember(d => d.SpecificProperty, m => m.Ignore())
                .Include<StandardDomain, Dto>();

            cfg.CreateMap<StandardDomain, Dto>()
                .Include<SpecificDomain, Dto>();

            cfg.CreateMap<SpecificDomain, Dto>()
                .Include<MoreSpecificDomain, Dto>();

            cfg.CreateMap<MoreSpecificDomain, Dto>();
        });

        config.AssertConfigurationIsValid();
    }
}
