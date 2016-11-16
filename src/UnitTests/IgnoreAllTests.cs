using System.Collections.Generic;
using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests
{
    public class When_overriding_global_ignore : AutoMapperSpecBase
    {
        Destination _destination;

        public class Source
        {
            public int ShouldBeMapped { get; set; }
        }

        public class Destination
        {
            public int ShouldBeMapped { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.AddGlobalIgnore("ShouldBeMapped");
            cfg.CreateMap<Source, Destination>().ForMember(d => d.ShouldBeMapped, o => o.UseValue(12));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_not_ignore()
        {
            _destination.ShouldBeMapped.ShouldEqual(12);
        }
    }

    public class IgnoreAllTests
    {
        public class Source
        {
            public string ShouldBeMapped { get; set; }
        }

        public class Destination
        {
            public string ShouldBeMapped { get; set; }
            public string StartingWith_ShouldNotBeMapped { get; set; }
            public List<string> StartingWith_ShouldBeNullAfterwards { get; set; }
            public string AnotherString_ShouldBeNullAfterwards { get; set; }
        }

        public class DestinationWrongType
        {
            public Destination ShouldBeMapped { get; set; }
        }

        public class FooProfile : Profile
        {
            public FooProfile()
            {
                CreateMap<Source, Destination>()
                    .ForMember(dest => dest.AnotherString_ShouldBeNullAfterwards, opt => opt.Ignore());
            }
        }

        [Fact]
        public void GlobalIgnore_ignores_all_properties_beginning_with_string()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGlobalIgnore("StartingWith");
                cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.AnotherString_ShouldBeNullAfterwards, opt => opt.Ignore());
            });
            
            config.CreateMapper().Map<Source, Destination>(new Source{ShouldBeMapped = "true"});
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void GlobalIgnore_ignores_all_properties_beginning_with_string_in_profiles()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGlobalIgnore("StartingWith");
                cfg.AddProfile<FooProfile>();
            });
            
            config.CreateMapper().Map<Source, Destination>(new Source{ShouldBeMapped = "true"});
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void GlobalIgnore_ignores_properties_with_names_matching_but_different_types()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGlobalIgnore("ShouldBeMapped");
                cfg.CreateMap<Source, DestinationWrongType>();
            });

            config.CreateMapper().Map<Source, DestinationWrongType>(new Source { ShouldBeMapped = "true" });
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void Ignored_properties_should_be_default_value()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGlobalIgnore("StartingWith");
                cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.AnotherString_ShouldBeNullAfterwards, opt => opt.Ignore());
            });

            Destination destination = config.CreateMapper().Map<Source, Destination>(new Source { ShouldBeMapped = "true" });
            destination.StartingWith_ShouldBeNullAfterwards.ShouldEqual(null);
            destination.StartingWith_ShouldNotBeMapped.ShouldEqual(null);
        }

        [Fact]
        public void Ignore_supports_two_different_values()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGlobalIgnore("StartingWith");
                cfg.AddGlobalIgnore("AnotherString");
                cfg.CreateMap<Source, Destination>();
            });

            Destination destination = config.CreateMapper().Map<Source, Destination>(new Source { ShouldBeMapped = "true" });
            destination.AnotherString_ShouldBeNullAfterwards.ShouldEqual(null);
            destination.StartingWith_ShouldNotBeMapped.ShouldEqual(null);
        }
    }
    public class IgnoreAttributeTests
    {
        public class Source
        {
            public string ShouldBeMapped { get; set; }
            public string ShouldNotBeMapped { get; set; }
        }

        public class Destination
        {
            public string ShouldBeMapped { get; set; }
            [IgnoreMap]
            public string ShouldNotBeMapped { get; set; }
        }

        [Fact]
        public void Ignore_On_Source_Field()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());
            config.AssertConfigurationIsValid();

            Source source = new Source
            {
                ShouldBeMapped = "Value1",
                ShouldNotBeMapped = "Value2"
            };

            Destination destination = config.CreateMapper().Map<Source, Destination>(source);
            destination.ShouldNotBeMapped.ShouldEqual(null);
        }
    }

    public class ReverseMapIgnoreAttributeTests
    {
        public class Source
        {
            public string ShouldBeMapped { get; set; }
            public string ShouldNotBeMapped { get; set; }
        }

        public class Destination
        {
            public string ShouldBeMapped { get; set; }
            [IgnoreMap]
            public string ShouldNotBeMapped { get; set; }
        }

        [Fact]
        public void Ignore_On_Source_Field()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
                .ReverseMap());
            config.AssertConfigurationIsValid();

            Destination source = new Destination
            {
                ShouldBeMapped = "Value1",
                ShouldNotBeMapped = "Value2"
            };

            Source destination = config.CreateMapper().Map<Destination, Source>(source);
            destination.ShouldNotBeMapped.ShouldEqual(null);

        }
    }
}