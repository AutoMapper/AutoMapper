using AutoMapper.Configuration.Conventions;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    namespace NamingConventions
    {
        public class RemoveNameSplitMapper : NonValidatingSpecBase
        {
            class Source
            {
                public InnerSource InnerSource { get; set; }
            }
            class InnerSource
            {
                public int Value { get; set; }
            }
            class Destination
            {
                public int InnerSourceValue { get; set; }
            }
            protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
            {
                var mappers = ((Profile)c).DefaultMemberConfig.MemberMappers;
                mappers.Remove(mappers.OfType<NameSplitMember>().Single());
                c.CreateMap<Source, Destination>();
            });
            [Fact]
            public void Should_not_validate() => Should.Throw<AutoMapperConfigurationException>(() => Configuration.AssertConfigurationIsValid())
                .Errors.Single().UnmappedPropertyNames.Single().ShouldBe(nameof(Destination.InnerSourceValue));
        }
        public class ExactMatchNamingConvention : NonValidatingSpecBase
        {
            class Source
            {
                public string Name { get; set; }
            }
            class Destination
            {
                public string Name { get; set; }
                public string COMPANY_Name { get; set; }
            }
            protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
            {
                cfg.DestinationMemberNamingConvention = new AutoMapper.ExactMatchNamingConvention();
                cfg.CreateMap<Source, Destination>();
            });
            [Fact]
            public void Should_not_use_pascal_naming_convention() =>
                new Action(Mapper.ConfigurationProvider.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>()
                    .Errors[0].UnmappedPropertyNames.ShouldContain("COMPANY_Name");
        }
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
                _neda.ja_se_zovem_imenom.ShouldBe("foo");
            }

            [Fact]
            public void Should_map_from_pascal_to_lower()
            {
                _dario.JaSeZovemImenom.ShouldBe("foo");
            }
        }

    }
}