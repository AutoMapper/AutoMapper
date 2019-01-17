using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class AddProfiles : SpecBase
    {
        public class Source { }
        public class Dest { }

        public class ForwardProfile : Profile
        {
            public ForwardProfile()
            {
                CreateMap<Source, Dest>();
            }
        }

        public class ReverseProfile : Profile
        {
            public ReverseProfile()
            {
                CreateMap<Dest, Source>();
            }
        }

        [Fact]
        public void Should_not_throw_when_loading_multiple_profiles()
        {
            IEnumerable<Profile> profiles = new Profile[] { new ForwardProfile(), new ReverseProfile() };
            var config = new MapperConfiguration(cfg => cfg.AddProfiles(profiles));

            config.AssertConfigurationIsValid();
        }

    }
}