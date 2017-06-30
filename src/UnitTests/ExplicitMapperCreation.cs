using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ExplicitMapperCreation : SpecBase
    {
        private IMapper _mapper;
        private MapperConfiguration _config;

        protected override void Establish_context()
        {
            _config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

            _mapper = _config.CreateMapper();
        }

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Should_map()
        {
            var source = new Source {Value = 10};
            var dest = _mapper.Map<Source, Dest>(source);

            dest.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void Should_have_valid_config()
        {
            _config.AssertConfigurationIsValid();
        }
    }
}