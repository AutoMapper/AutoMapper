using System;
using System.Linq.Expressions;
using Xunit;
using Should;

namespace AutoMapper.UnitTests.Constructors
{
    public class When_renaming_class_constructor_parameter : AutoMapperSpecBase
    {
        Destination _destination;

        public class Source
        {
            public InnerSource InnerSource { get; set; }
        }

        public class InnerSource
        {
            public string Name { get; set; }
        }

        public class Destination
        {
            public Destination(InnerDestination inner)
            {
                InnerDestination = inner;
            }

            public InnerDestination InnerDestination { get; }
        }

        public class InnerDestination
        {
            public string Name { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            c.CreateMap<Source, Destination>().ForCtorParam("inner", o=>o.MapFrom(s=>s.InnerSource));
            c.CreateMap<InnerSource, InnerDestination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { InnerSource = new InnerSource { Name = "Core" } });
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.InnerDestination.Name.ShouldEqual("Core");
        }
    }

    public class When_constructor_matches_with_prefix_and_postfix : AutoMapperSpecBase
    {
        PersonDto _destination;

        public class Person
        {
            public string PrefixNamePostfix { get; set; }
        }

        public class PersonDto
        {
            string name;

            public PersonDto(string name)
            {
                this.name = name;
            }

            public string Name => name;
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.RecognizePostfixes("postfix");
            cfg.RecognizePrefixes("prefix");

            cfg.CreateMap<Person, PersonDto>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<PersonDto>(new Person { PrefixNamePostfix = "John" });
        }

        [Fact]
        public void Should_map_from_the_property()
        {
            _destination.Name.ShouldEqual("John");
        }
    }

    public class When_constructor_matches_with_destination_prefix_and_postfix : AutoMapperSpecBase
    {
        PersonDto _destination;

        public class Person
        {
            public string Name { get; set; }
        }

        public class PersonDto
        {
            string name;

            public PersonDto(string prefixNamePostfix)
            {
                name = prefixNamePostfix;
            }

            public string Name => name;
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.RecognizeDestinationPostfixes("postfix");
            cfg.RecognizeDestinationPrefixes("prefix");

            cfg.CreateMap<Person, PersonDto>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<PersonDto>(new Person { Name = "John" });
        }

        [Fact]
        public void Should_map_from_the_property()
        {
            _destination.Name.ShouldEqual("John");
        }
    }

    public class When_constructor_matches_but_is_overriden_by_ConstructUsing : AutoMapperSpecBase
    {
        PersonDto _destination;

        public class Person
        {
            public string Name { get; set; }
        }

        public class PersonDto
        {
            public PersonDto()
            {
            }

            public PersonDto(string name)
            {
                Name = name;
            }

            public string Name { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDto>().ConstructUsing(p=>new PersonDto()));

        protected override void Because_of()
        {
            _destination = Mapper.Map<PersonDto>(new Person { Name = "John" });
        }

        [Fact]
        public void Should_map_from_the_property()
        {
            _destination.Name.ShouldEqual("John");
        }
    }

    public class When_constructor_is_match_with_default_value : AutoMapperSpecBase
    {
        PersonDto _destination;

        public class Person
        {
            public string Name { get; set; }
        }

        public class PersonDto
        {
            public PersonDto(string name = null)
            {
                Name = name;
            }

            public string Name { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDto>());

        protected override void Because_of()
        {
            _destination = Mapper.Map<PersonDto>(new Person { Name = "John" });
        }

        [Fact]
        public void Should_map_from_the_property()
        {
            _destination.Name.ShouldEqual("John");
        }
    }

    public class When_constructor_is_partial_match_with_value_type : AutoMapperSpecBase
    {
        GeoCoordinate _destination;

        public class GeolocationDTO
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public double? HorizontalAccuracy { get; set; }
        }

        public struct GeoCoordinate
        {
            public GeoCoordinate(double longitude, double latitude, double x)
            {
                Longitude = longitude;
                Latitude = latitude;
                HorizontalAccuracy = 0;
            }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public double? HorizontalAccuracy { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<GeoCoordinate, GeolocationDTO>();
            cfg.CreateMap<GeolocationDTO, GeoCoordinate>();
        });

        protected override void Because_of()
        {
            var source = new GeolocationDTO
            {
                Latitude = 34d,
                Longitude = -93d,
                HorizontalAccuracy = 100
            };
            _destination = Mapper.Map<GeoCoordinate>(source);
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Latitude.ShouldEqual(34);
            _destination.Longitude.ShouldEqual(-93);
            _destination.HorizontalAccuracy.ShouldEqual(100);
        }
    }

    public class When_constructor_is_partial_match : AutoMapperSpecBase
    {
        GeoCoordinate _destination;

        public class GeolocationDTO
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public double? HorizontalAccuracy { get; set; }
        }

        public class GeoCoordinate
        {
            public GeoCoordinate()
            {
            }
            public GeoCoordinate(double longitude, double latitude, double x)
            {
                Longitude = longitude;
                Latitude = latitude;
            }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public double? HorizontalAccuracy { get; set; }
            public double Altitude { get; set; }
            public double VerticalAccuracy { get; set; }
            public double Speed { get; set; }
            public double Course { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<GeoCoordinate, GeolocationDTO>();

            cfg.CreateMap<GeolocationDTO, GeoCoordinate>()
                .ForMember(dest => dest.Altitude, opt => opt.Ignore())
                .ForMember(dest => dest.VerticalAccuracy, opt => opt.Ignore())
                .ForMember(dest => dest.Speed, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore());
        });

        protected override void Because_of()
        {
            var source = new GeolocationDTO
            {
                Latitude = 34d,
                Longitude = -93d,
                HorizontalAccuracy = 100
            };
            _destination = Mapper.Map<GeoCoordinate>(source);
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Latitude.ShouldEqual(34);
            _destination.Longitude.ShouldEqual(-93);
            _destination.HorizontalAccuracy.ShouldEqual(100);
        }
    }

    public class When_constructor_matches_but_the_destination_is_passed : AutoMapperSpecBase
    {
        Destination _destination = new Destination();

        public class Source
        {
            public int MyTypeId { get; set; }
        }

        public class MyType
        {
        }

        public class Destination
        {
            private MyType _myType;

            public Destination()
            {

            }
            public Destination(MyType myType)
            {
                _myType = myType;
            }

            public MyType MyType
            {
                get { return _myType; }
                set { _myType = value; }
            }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(cfg =>
                {
                    cfg.RecognizePostfixes("Id");
                    cfg.CreateMap<Source, Destination>();
                    cfg.CreateMap<int, MyType>();
                });
            }
        }

        protected override void Because_of()
        {
            Mapper.Map(new Source(), _destination);
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.MyType.ShouldNotBeNull();
        }
    }

    public class When_mapping_through_constructor_and_destination_has_setter : AutoMapperSpecBase
    {
        public class Source
        {
            public int MyTypeId { get; set; }
        }

        public class MyType
        {
        }

        Destination _destination;
        public class Destination
        {
            private MyType _myType;

            private Destination()
            {

            }
            public Destination(MyType myType)
            {
                _myType = myType;
            }

            public MyType MyType
            {
                get { return _myType; }
                private set
                {
                    throw new Exception("Should not set through setter.");
                }
            }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(cfg =>
                {
                    cfg.RecognizePostfixes("Id");
                    cfg.CreateMap<Source, Destination>();
                    cfg.CreateMap<int, MyType>();

                });
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.MyType.ShouldNotBeNull();
        }
    }

    public class When_mapping_an_optional_GUID_constructor : AutoMapperSpecBase
    {
        Destination _destination;

        public class Destination
        {
            public Destination(Guid id = default(Guid)) { Id = id; }
            public Guid Id { get; set; }
        }

        public class Source
        {
            public Guid Id { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(c=>c.CreateMap<Source, Destination>());
            }
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Id.ShouldEqual(Guid.Empty);
        }
    }

    public class When_mapping_a_constructor_parameter_from_nested_members : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public NestedSource Nested { get; set; }
        }

        public class NestedSource
        {
            public int Foo { get; set; }
        }

        public class Destination
        {
            public int Foo { get; }

            public Destination(int foo)
            {
                Foo = foo;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ForCtorParam("foo", opt => opt.MapFrom(s => s.Nested.Foo));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { Nested = new NestedSource { Foo = 5 } });
        }

        [Fact]
        public void Should_map_the_constructor_argument()
        {
            _destination.Foo.ShouldEqual(5);
        }
    }

    public class When_the_destination_has_a_matching_constructor_with_optional_extra_parameters : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int Foo { get; set; }
        }

        public class Destination
        {
            private readonly int _foo;

            public int Foo
            {
                get { return _foo; }
            }

            public string Bar { get;}

            public Destination(int foo, string bar = "bar")
            {
                _foo = foo;
                Bar = bar;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Foo = 5 });
        }

        [Fact]
        public void Should_map_the_constructor_argument()
        {
            _destination.Foo.ShouldEqual(5);
            _destination.Bar.ShouldEqual("bar");
        }
    }

    public class When_mapping_to_an_object_with_a_constructor_with_a_matching_argument : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }

        public class Dest
        {
            private readonly int _foo;

            public int Foo
            {
                get { return _foo; }
            }

            public int Bar { get; set; }

            public Dest(int foo)
            {
                _foo = foo;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });

        protected override void Because_of()
        {
            Expression<Func<object, object>> ctor = (input) => new Dest((int)input);

            object o = ctor.Compile()(5);

            _dest = Mapper.Map<Source, Dest>(new Source { Foo = 5, Bar = 10 });
        }

        [Fact]
        public void Should_map_the_constructor_argument()
        {
            _dest.Foo.ShouldEqual(5);
        }

        [Fact]
        public void Should_map_the_existing_properties()
        {
            _dest.Bar.ShouldEqual(10);
        }
    }

    public class When_mapping_to_an_object_with_a_private_constructor : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public int Foo { get; set; }
        }

        public class Dest
        {
            private readonly int _foo;

            public int Foo
            {
                get { return _foo; }
            }

            private Dest(int foo)
            {
                _foo = foo;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });

        protected override void Because_of()
        {
            _dest = Mapper.Map<Source, Dest>(new Source { Foo = 5 });
        }

        [Fact]
        public void Should_map_the_constructor_argument()
        {
            _dest.Foo.ShouldEqual(5);
        }
    }

    public class When_mapping_to_an_object_using_service_location : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public int Foo { get; set; }
        }

        public class Dest
        {
            private int _foo;
            private readonly int _addend;

            public int Foo
            {
                get { return _foo + _addend; }
                set { _foo = value; }
            }

            public Dest(int addend)
            {
                _addend = addend;
            }

            public Dest()
                : this(0)
            {
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.ConstructServicesUsing(t => new Dest(5));
            cfg.CreateMap<Source, Dest>()
                .ConstructUsingServiceLocator();
        });

        protected override void Because_of()
        {
            _dest = Mapper.Map<Source, Dest>(new Source { Foo = 5 });
        }

        [Fact]
        public void Should_map_with_the_custom_constructor()
        {
            _dest.Foo.ShouldEqual(10);
        }
    }

    public class When_mapping_to_an_object_using_contextual_service_location : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public int Foo { get; set; }
        }

        public class Dest
        {
            private int _foo;
            private readonly int _addend;

            public int Foo
            {
                get { return _foo + _addend; }
                set { _foo = value; }
            }

            public Dest(int addend)
            {
                _addend = addend;
            }

            public Dest()
                : this(0)
            {
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.ConstructServicesUsing(t => new Dest(5));
            cfg.CreateMap<Source, Dest>()
                .ConstructUsingServiceLocator();
        });

        protected override void Because_of()
        {
            _dest = Mapper.Map<Source, Dest>(new Source { Foo = 5 }, opt => opt.ConstructServicesUsing(t => new Dest(6)));
        }

        [Fact]
        public void Should_map_with_the_custom_constructor()
        {
            _dest.Foo.ShouldEqual(11);
        }
    }

    public class When_mapping_to_an_object_with_multiple_constructors_and_constructor_mapping_is_disabled : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }

        public class Dest
        {
            public int Foo { get; set; }

            public int Bar { get; set; }

            public Dest(int foo)
            {
                throw new NotImplementedException();
            }

            public Dest() { }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.DisableConstructorMapping();
            cfg.CreateMap<Source, Dest>();
        });

        protected override void Because_of()
        {
            _dest = Mapper.Map<Source, Dest>(new Source { Foo = 5, Bar = 10 });
        }

        [Fact]
        public void Should_map_the_existing_properties()
        {
            _dest.Foo.ShouldEqual(5);
            _dest.Bar.ShouldEqual(10);
        }
    }
    public class UsingMappingEngineToResolveConstructorArguments
    {
        [Fact]
        public void Should_resolve_constructor_arguments_using_mapping_engine()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceBar, DestinationBar>();

                cfg.CreateMap<SourceFoo, DestinationFoo>();
            });

            var sourceBar = new SourceBar("fooBar");
            var sourceFoo = new SourceFoo(sourceBar);

            var destinationFoo = config.CreateMapper().Map<DestinationFoo>(sourceFoo);

            destinationFoo.Bar.FooBar.ShouldEqual(sourceBar.FooBar);
        }


        public class DestinationFoo
        {
            private readonly DestinationBar _bar;

            public DestinationBar Bar
            {
                get { return _bar; }
            }

            public DestinationFoo(DestinationBar bar)
            {
                _bar = bar;
            }
        }

        public class DestinationBar
        {
            private readonly string _fooBar;

            public string FooBar
            {
                get { return _fooBar; }
            }

            public DestinationBar(string fooBar)
            {
                _fooBar = fooBar;
            }
        }

        public class SourceFoo
        {
            public SourceBar Bar { get; private set; }

            public SourceFoo(SourceBar bar)
            {
                Bar = bar;
            }
        }

        public class SourceBar
        {
            public string FooBar { get; private set; }

            public SourceBar(string fooBar)
            {
                FooBar = fooBar;
            }
        }
    }

    public class MappingMultipleConstructorArguments
    {
        [Fact]
        public void Should_resolve_constructor_arguments_using_mapping_engine()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceBar, DestinationBar>();

                cfg.CreateMap<SourceFoo, DestinationFoo>();
            });

            var sourceBar = new SourceBar("fooBar");
            var sourceFoo = new SourceFoo(sourceBar, new SourceBar("fooBar2"));

            var destinationFoo = config.CreateMapper().Map<DestinationFoo>(sourceFoo);

            destinationFoo.Bar.FooBar.ShouldEqual(sourceBar.FooBar);
            destinationFoo.Bar2.FooBar.ShouldEqual("fooBar2");
        }


        public class DestinationFoo
        {
            private readonly DestinationBar _bar;

            public DestinationBar Bar
            {
                get { return _bar; }
            }

            public DestinationBar Bar2 { get; private set; }

            public DestinationFoo(DestinationBar bar, DestinationBar bar2)
            {
                _bar = bar;
                Bar2 = bar2;
            }
        }

        public class DestinationBar
        {
            private readonly string _fooBar;

            public string FooBar
            {
                get { return _fooBar; }
            }

            public DestinationBar(string fooBar)
            {
                _fooBar = fooBar;
            }
        }

        public class SourceFoo
        {
            public SourceBar Bar { get; private set; }
            public SourceBar Bar2 { get; private set; }

            public SourceFoo(SourceBar bar, SourceBar bar2)
            {
                Bar = bar;
                Bar2 = bar2;
            }
        }

        public class SourceBar
        {
            public string FooBar { get; private set; }

            public SourceBar(string fooBar)
            {
                FooBar = fooBar;
            }
        }
    }

    public class When_mapping_to_an_object_with_a_constructor_with_multiple_optional_arguments
    {
        [Fact]
        public void Should_resolve_constructor_when_args_are_optional()
        {

            var config = new MapperConfiguration(cfg => cfg.CreateMap<SourceFoo, DestinationFoo>());

            var sourceBar = new SourceBar("fooBar");
            var sourceFoo = new SourceFoo(sourceBar);

            var destinationFoo = config.CreateMapper().Map<DestinationFoo>(sourceFoo);

            destinationFoo.Bar.ShouldBeNull();
            destinationFoo.Str.ShouldEqual("hello");
        }


        public class DestinationFoo
        {
            private readonly DestinationBar _bar;
            private string _str;

            public DestinationBar Bar
            {
                get { return _bar; }
            }

            public string Str
            {
                get { return _str; }
            }

            public DestinationFoo(DestinationBar bar=null,string str="hello")
            {
                _bar = bar;
                _str = str;
            }
        }

        public class DestinationBar
        {
            private readonly string _fooBar;

            public string FooBar
            {
                get { return _fooBar; }
            }

            public DestinationBar(string fooBar)
            {
                _fooBar = fooBar;
            }
        }

        public class SourceFoo
        {
            public SourceBar Bar { get; private set; }

            public SourceFoo(SourceBar bar)
            {
                Bar = bar;
            }
        }

        public class SourceBar
        {
            public string FooBar { get; private set; }

            public SourceBar(string fooBar)
            {
                FooBar = fooBar;
            }
        }
    }


    public class When_mapping_to_an_object_with_a_constructor_with_single_optional_arguments
    {
        [Fact]
        public void Should_resolve_constructor_when_arg_is_optional()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<SourceFoo, DestinationFoo>());

            var sourceBar = new SourceBar("fooBar");
            var sourceFoo = new SourceFoo(sourceBar);

            var destinationFoo = config.CreateMapper().Map<DestinationFoo>(sourceFoo);

            destinationFoo.Bar.ShouldBeNull();
        }


        public class DestinationFoo
        {
            private readonly DestinationBar _bar;

            public DestinationBar Bar
            {
                get { return _bar; }
            }

            public DestinationFoo(DestinationBar bar = null)
            {
                _bar = bar;
            }
        }

        public class DestinationBar
        {
            private readonly string _fooBar;

            public string FooBar
            {
                get { return _fooBar; }
            }

            public DestinationBar(string fooBar)
            {
                _fooBar = fooBar;
            }
        }

        public class SourceFoo
        {
            public SourceBar Bar { get; private set; }

            public SourceFoo(SourceBar bar)
            {
                Bar = bar;
            }
        }

        public class SourceBar
        {
            public string FooBar { get; private set; }

            public SourceBar(string fooBar)
            {
                FooBar = fooBar;
            }
        }
    }

    public class When_mapping_to_an_object_with_a_constructor_with_string_optional_arguments
    {
        [Fact]
        public void Should_resolve_constructor_when_string_args_are_optional()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<SourceFoo, DestinationFoo>());

            var sourceBar = new SourceBar("fooBar");
            var sourceFoo = new SourceFoo(sourceBar);

            var destinationFoo = config.CreateMapper().Map<DestinationFoo>(sourceFoo);

            destinationFoo.A.ShouldEqual("a");
            destinationFoo.B.ShouldEqual("b");
            destinationFoo.C.ShouldEqual(3);
        }


        public class DestinationFoo
        {
            private string _a;
            private string _b;
            private int _c;
            public string A
            {
                get { return _a; }
            }

            public string B
            {
                get { return _b; }
            }

            public int C
            {
                get { return _c; }
            }

            public DestinationFoo(string a = "a",string b="b", int c = 3)
            {
                _a = a;
                _b = b;
                _c = c;
            }
        }

        public class DestinationBar
        {
            private readonly string _fooBar;

            public string FooBar
            {
                get { return _fooBar; }
            }

            public DestinationBar(string fooBar)
            {
                _fooBar = fooBar;
            }
        }

        public class SourceFoo
        {
            public SourceBar Bar { get; private set; }

            public SourceFoo(SourceBar bar)
            {
                Bar = bar;
            }
        }

        public class SourceBar
        {
            public string FooBar { get; private set; }

            public SourceBar(string fooBar)
            {
                FooBar = fooBar;
            }
        }
    }

    public class When_configuring_ctor_param_members : AutoMapperSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest(int thing)
            {
                Value1 = thing;
            }

            public int Value1 { get; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt => opt.MapFrom(src => src.Value));
        });

        [Fact]
        public void Should_redirect_value()
        {
            var dest = Mapper.Map<Source, Dest>(new Source {Value = 5});

            dest.Value1.ShouldEqual(5);
        }
    }

    public class When_configuring_nullable_ctor_param_members : AutoMapperSpecBase
    {
        public class Source
        {
            public int? Value { get; set; }
        }

        public class Dest
        {
            public Dest(int? thing)
            {
                Value1 = thing;
            }

            public int? Value1 { get; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt => opt.ResolveUsing(src => src.Value));
        });

        [Fact]
        public void Should_redirect_value()
        {
            var dest = Mapper.Map<Source, Dest>(new Source());

            dest.Value1.ShouldBeNull();
        }
    }
}
