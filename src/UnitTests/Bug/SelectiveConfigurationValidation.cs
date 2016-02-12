﻿namespace AutoMapper.UnitTests.Bug
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

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<GoodProfile>();
            cfg.AddProfile<BadProfile>();
        });

        [Fact]
        public void Should_pass_specific_profile_assertion()
        {
            typeof(AutoMapperConfigurationException)
                .ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid<GoodProfile>);
        }
    }
}