using System;
using Xunit;
using Should;
using System.Reflection;

namespace AutoMapper.UnitTests
{
    namespace ExtensionMethods
    {
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;

        public static class When_extension_method_returns_value_type_SourceExtensions
        {
            public static string GetValue2(this When_extension_method_returns_value_type.Source source) { return "hello from extension"; }
        }

        public class When_extension_method_returns_value_type : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public int Value1 { get; set; }
            }

            public struct Destination
            {
                public int Value1 { get; set; }
                public string Value2 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.IncludeSourceExtensionMethods(typeof(When_extension_method_returns_value_type_SourceExtensions));
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source { Value1 = 3 });
            }

            [Fact]
            public void Should_use_extension_method()
            {
                _destination.Value2.ShouldEqual("hello from extension");
            }

            [Fact]
            public void Should_still_map_value_type()
            {
                _destination.Value1.ShouldEqual(3);
            }
        }

        public static class When_extension_method_returns_object_SourceExtensions
        {
            public static When_extension_method_returns_object.Nested GetInsideThing(this When_extension_method_returns_object.Source source)
            {
                return new When_extension_method_returns_object.Nested { Property = source.Value1 + 10 };
            }
        }

        public class When_extension_method_returns_object : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public int Value1 { get; set; }
            }

            public struct Destination
            {
                public int Value1 { get; set; }
                public int InsideThingProperty { get; set; }
            }

            public class Nested
            {
                public int Property { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.IncludeSourceExtensionMethods(typeof(When_extension_method_returns_object_SourceExtensions));
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source { Value1 = 7 });
            }

            [Fact]
            public void Should_flatten_using_extension_method()
            {
                _destination.InsideThingProperty.ShouldEqual(17);
            }

            [Fact]
            public void Should_still_map_value_type()
            {
                _destination.Value1.ShouldEqual(7);
            }
        }

        public class When_extension_contains_LINQ_methods : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public IEnumerable<int> Values { get; set; }
            }

            public class Destination
            {
                public int ValuesCount { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source {Values = Enumerable.Repeat(1, 10)});
            }

            [Fact]
            public void Should_resolve_LINQ_method_automatically()
            {
                _destination.ValuesCount.ShouldEqual(10);
            }
        }
    }
}
