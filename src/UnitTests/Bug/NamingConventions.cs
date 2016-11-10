using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    namespace NamingConventions
    {
        public class Neda
        {
            public string cmok { get; set; }

            public string moje_ime { get; set; }

            public string moje_prezime { get; set; }

            public string ja_se_zovem_imenom { get; set; }

        }

        public class Dario
        {
            public string cmok { get; set; }

            public string MojeIme { get; set; }

            public string MojePrezime { get; set; }

            public string JaSeZovemImenom { get; set; }
        }

        public class When_mapping_with_lowercae_naming_conventions_two_ways_in_profiles : AutoMapperSpecBase
        {
            private Dario _dario;
            private Neda _neda;

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateProfile("MyMapperProfile", prf =>
                {
                    prf.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
                    prf.CreateMap<Neda, Dario>();
                });
                cfg.CreateProfile("MyMapperProfile2", prf =>
                {
                    prf.DestinationMemberNamingConvention = new LowerUnderscoreNamingConvention();
                    prf.CreateMap<Dario, Neda>();
                });
            });

            protected override void Because_of()
            {
                _dario = Mapper.Map<Neda, Dario>(new Neda {ja_se_zovem_imenom = "foo"});
                _neda = Mapper.Map<Dario, Neda>(_dario);
            }

            [Fact]
            public void Should_map_from_lower_to_pascal()
            {
                _neda.ja_se_zovem_imenom.ShouldEqual("foo");
            }

            [Fact]
            public void Should_map_from_pascal_to_lower()
            {
                _dario.JaSeZovemImenom.ShouldEqual("foo");
            }
        }

    }
}