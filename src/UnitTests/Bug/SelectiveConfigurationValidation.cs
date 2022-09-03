namespace AutoMapper.UnitTests.Bug;
public class SelectiveConfigurationValidation : NonValidatingSpecBase
{
    public class GoodSrc { }
    public class GoodDest { }

    public class BadSrc
    {
        public Type BlowUp { get; set; }
    }

    public class BadDest
    {
        public int Value { get; set; }
        public int BlowUp { get; set; }
    }
    public class GoodProfile : Profile
    {
        public GoodProfile()
        {
            CreateMap<GoodSrc, GoodDest>();
        }
    }

    public class BadProfile : Profile
    {
        public BadProfile()
        {
            CreateMap<BadSrc, BadDest>();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AddProfile<GoodProfile>();
        cfg.AddProfile<BadProfile>();
    });

    [Fact]
    public void Should_pass_specific_profile_assertion()
    {
        typeof(AutoMapperConfigurationException)
            .ShouldNotBeThrownBy(AssertConfigurationIsValid<GoodProfile>);
    }
}