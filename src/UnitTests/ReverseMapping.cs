using NUnit.Framework;
using Should;
using System.Linq;

namespace AutoMapper.UnitTests
{
    namespace ReverseMapping
    {
        public class When_reverse_mapping_classes_with_simple_properties : AutoMapperSpecBase
        {
            private Source _source;

            public class Source
            {
                public int Value { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ReverseMap();
                });
            }

            protected override void Because_of()
            {
                var dest = new Destination
                {
                    Value = 10
                };
                _source = Mapper.Map<Destination, Source>(dest);
            }

            [Test]
            public void Should_create_a_map_with_the_reverse_items()
            {
                _source.Value.ShouldEqual(10);
            }
        }

        public class When_validating_only_against_source_members_and_source_matches : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source);
                });
            }

            [Test]
            public void Should_only_map_source_members()
            {
                var typeMap = Mapper.FindTypeMapFor<Source, Destination>();

                typeMap.GetPropertyMaps().Count().ShouldEqual(1);
            }

            [Test]
            public void Should_not_throw_any_configuration_validation_errors()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
            }
        }

        public class When_validating_only_against_source_members_and_source_does_not_match : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source);
                });
            }

            [Test]
            public void Should_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
            }
        }

        public class When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
                public int Value3 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source)
                        .ForMember(dest => dest.Value3, opt => opt.MapFrom(src => src.Value2));
                });
            }

            [Test]
            public void Should_not_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
            }
        }

        public class When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped_with_resolvers : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
                public int Value3 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source)
                        .ForMember(dest => dest.Value3, opt => opt.ResolveUsing(src => src.Value2))
                        .ForSourceMember(src => src.Value2, opt => opt.Ignore());
                });
            }

            [Test]
            public void Should_not_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
            }
        }

    }
}