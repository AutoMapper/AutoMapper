using Xunit;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AutoMapper.UnitTests
{
    public class When_an_extension_method_is_for_a_base_class : AutoMapperSpecBase
    {
        class Source
        {
        }
        class Destination
        {
            public int Value { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(c=>
        {
            c.IncludeSourceExtensionMethods(typeof(BarExtensions));
            c.CreateMap<Source, Destination>();
        });
        [Fact]
        public void It_should_be_used() => Map<Destination>(new Source()).Value.ShouldBe(12);
    }
    public static class BarExtensions
    {
        public static int GetValue(this object obj) => 12;
        public static string GetSimpleName(this When_null_is_passed_to_an_extension_method.Bar source)
        {
            if(source == null)
                throw new ArgumentNullException("source");
            return "SimpleName";
        }
    }

    public class When_null_is_passed_to_an_extension_method : AutoMapperSpecBase
    {
        public class Foo
        {
            public Bar Bar { get; set; }
        }

        public class Bar
        {
            public string Name { get; set; }
        }

        public class FooDto
        {
            public string BarSimpleName { get; set; }
            public Guid Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.IncludeSourceExtensionMethods(typeof(BarExtensions));
            cfg.CreateMap<Foo, FooDto>().ForMember(d=>d.Value, o=>o.MapFrom(s=>Guid.NewGuid()));
        });

        [Fact]
        public void Should_work()
        {
            Mapper.Map<FooDto>(new Foo()).BarSimpleName.ShouldBeNull();
        }
    }

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
            _destination.Value2.ShouldBe("hello from extension");
        }

        [Fact]
        public void Should_still_map_value_type()
        {
            _destination.Value1.ShouldBe(3);
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
            _destination.InsideThingProperty.ShouldBe(17);
        }

        [Fact]
        public void Should_still_map_value_type()
        {
            _destination.Value1.ShouldBe(7);
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
            _destination = Mapper.Map<Source, Destination>(new Source { Values = Enumerable.Repeat(1, 10) });
        }

        [Fact]
        public void Should_resolve_LINQ_method_automatically()
        {
            _destination.ValuesCount.ShouldBe(10);
        }
    }

    public class When_a_static_method_has_first_parameter_null : AutoMapperSpecBase
    {
        class FirstName
        {
            public string Name;
        }

        class LastName
        {
            public string Name;
        }

        class FullName
        {
            public string Name;
        }

        class CombinedNames
        {
            public FirstName First;

            public LastName Last;
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMap<CombinedNames, FullName>()
                .ForMember(dst => dst.Name, o => o.MapFrom(src => string.Concat(src.First.Name, src.Last.Name)));
        });

        [Fact]
        public void It_should_not_be_null_checked()
        {
            var combinedNames = new CombinedNames
            {
                First = new FirstName { Name = null },
                Last = new LastName { Name = "Doe" }
            };
            var fullName = Mapper.Map<FullName>(combinedNames);
            fullName.Name.ShouldBe("Doe");
        }
    }
}
