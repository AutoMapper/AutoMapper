using Xunit;
using Should;
using System.Linq;

namespace AutoMapper.UnitTests
{
    namespace NonGenericReverseMapping
    {
        using System;
        using System.Text.RegularExpressions;

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

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(typeof (Source), typeof (Destination)).ReverseMap();
            });

            protected override void Because_of()
            {
                var dest = new Destination
                {
                    Value = 10
                };
                _source = Mapper.Map<Destination, Source>(dest);
            }

            [Fact]
            public void Should_create_a_map_with_the_reverse_items()
            {
                _source.Value.ShouldEqual(10);
            }
        }

        public class When_reverse_mapping_and_ignoring_via_method : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
                public int Ignored { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(typeof (Source), typeof (Dest))
                    .ForMember("Ignored", opt => opt.Ignore())
                    .ReverseMap();
            });

            [Fact]
            public void Should_show_valid()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid());
            }
        }

        public class When_reverse_mapping_and_ignoring : SpecBase
        {
            public class Foo
            {
                public string Bar { get; set; }
                public string Baz { get; set; }
            }

            public class Foo2
            {
                public string Bar { get; set; }
                public string Boo { get; set; }
            }

            [Fact]
            public void GetUnmappedPropertyNames_ShouldReturnBoo()
            {
                //Arrange
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap(typeof(Foo), typeof(Foo2));
                });
                var typeMap = config.GetAllTypeMaps()
                          .First(x => x.SourceType == typeof(Foo) && x.DestinationType == typeof(Foo2));
                //Act
                var unmappedPropertyNames = typeMap.GetUnmappedPropertyNames();
                //Assert
                unmappedPropertyNames[0].ShouldEqual("Boo");
            }

            [Fact]
            public void WhenSecondCallTo_GetUnmappedPropertyNames_ShouldReturnBoo()
            {
                //Arrange
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap(typeof (Foo), typeof (Foo2)).ReverseMap();
                });
                var typeMap = config.GetAllTypeMaps()
                          .First(x => x.SourceType == typeof(Foo2) && x.DestinationType == typeof(Foo));
                //Act
                var unmappedPropertyNames = typeMap.GetUnmappedPropertyNames();
                //Assert
                unmappedPropertyNames[0].ShouldEqual("Boo");
            }
        }
    }
}