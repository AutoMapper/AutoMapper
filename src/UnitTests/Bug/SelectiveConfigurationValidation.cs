namespace AutoMapper.UnitTests.Bug
{
    using System;
    using Xunit;

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
            protected override void Configure()
            {
                CreateMap<GoodSrc, GoodDest>();
            }
        }

        public class BadProfile : Profile
        {
            protected override void Configure()
            {
                CreateMap<BadSrc, BadDest>();
            }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<GoodProfile>();
                cfg.AddProfile<BadProfile>();
            });
        }

        [Fact]
        public void Should_pass_specific_profile_assertion()
        {
            typeof(AutoMapperConfigurationException)
                .ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid<GoodProfile>);
        }
    }
}