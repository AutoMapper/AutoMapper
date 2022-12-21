namespace AutoMapper.UnitTests.Constructors;
public class RecordConstructorValidation : AutoMapperSpecBase
{
    record Destination(int Value) { }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<string, Destination>());
    [Fact]
    public void Validate() => new Action(AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>().Message.
        ShouldContainWithoutWhitespace("When mapping to records, consider using only public constructors.");
}
public class RecordConstructorValidationForCtorParam : AutoMapperSpecBase
{
    record Destination(int Value, int Other){}
    protected override MapperConfiguration CreateConfiguration() => new(c =>
        c.CreateMap<string, Destination>().ForCtorParam(nameof(Destination.Value), o => o.MapFrom(s => 0)));
    [Fact]
    public void Validate() => new Action(AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>().Message.
        ShouldContainWithoutWhitespace("When mapping to records, consider using only public constructors.");
}
public class ConstructorValidation : AutoMapperSpecBase
{
    class Source
    {
    }
    class Destination
    {
        public Destination(int otherValue, int value = 2) { }
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => 
        c.CreateMap<Source, Destination>().ForCtorParam("otherValue", o=>o.MapFrom(s=>0)));
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}
public class Nullable_enum_default_value : AutoMapperSpecBase
{
    public enum SourceEnum { A, B }
    public class Source
    {
        public SourceEnum? Enum { get; set; }
    }
    public enum TargetEnum { A, B }
    public class Target
    {
        public TargetEnum? Enum { get; set; }
        public Target(TargetEnum? Enum = TargetEnum.A)
        {
            this.Enum = Enum;
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>cfg.CreateMap<Source, Target>());
    [Fact]
    public void Should_work() => Mapper.Map<Target>(new Source { Enum = SourceEnum.B }).Enum.ShouldBe(TargetEnum.B);
}
public class Nullable_enum_default_value_null : AutoMapperSpecBase
{
    public class Source
    {
    }
    public enum TargetEnum { A, B }
    public class Target
    {
        public TargetEnum? Enum { get; }
        public Target(TargetEnum? Enum = null)
        {
            this.Enum = Enum;
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Target>());
    [Fact]
    public void Should_work() => Mapper.Map<Target>(new Source()).Enum.ShouldBeNull();
}
public class Nullable_enum_default_value_not_null : AutoMapperSpecBase
{
    public class Source
    {
    }
    public enum TargetEnum { A, B }
    public class Target
    {
        public TargetEnum? Enum { get; }
        public Target(TargetEnum? Enum = TargetEnum.B)
        {
            this.Enum = Enum;
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Target>());
    [Fact]
    public void Should_work() => Mapper.Map<Target>(new Source()).Enum.ShouldBe(TargetEnum.B);
}
public class Dynamic_constructor_mapping : AutoMapperSpecBase
{
    public class ParentDTO<T>
    {
        public ChildDTO<T> First => Children[0];
        public List<ChildDTO<T>> Children { get; set; } = new List<ChildDTO<T>>();
        public int IdParent { get; set; }
    }

    public class ChildDTO<T>
    {
        public int IdChild { get; set; }
        public ParentDTO<T> Parent { get; set; }
    }

    public class ParentModel<T>
    {
        public ChildModel<T> First { get; set; }
        public List<ChildModel<T>> Children { get; set; } = new List<ChildModel<T>>();
        public int IdParent { get; set; }
    }

    public class ChildModel<T>
    {
        int _idChild;

        public ChildModel(ParentModel<T> parent)
        {
            Parent = parent;
        }

        public int IdChild
        {
            get => _idChild;
            set
            {
                if (_idChild != 0)
                {
                    throw new Exception("Set IdChild again.");
                }
                _idChild = value;
            }
        }
        public ParentModel<T> Parent { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(ParentModel<>), typeof(ParentDTO<>)).ReverseMap();
        cfg.CreateMap(typeof(ChildModel<>), typeof(ChildDTO<>)).ReverseMap();
    });

    [Fact]
    public void Should_work()
    {
        var parentDto = new ParentDTO<int> { IdParent = 1 };
        for (var i = 0; i < 5; i++)
        {
            parentDto.Children.Add(new ChildDTO<int> { IdChild = i, Parent = parentDto });
        }
        var parentModel = Mapper.Map<ParentModel<int>>(parentDto);
        var mappedChildren = Mapper.Map<List<ChildDTO<int>>, List<ChildModel<int>>>(parentDto.Children);
    }
}

public class Constructor_mapping_without_preserve_references : AutoMapperSpecBase
{
    public class ParentDTO
    {
        public ChildDTO First => Children[0];
        public List<ChildDTO> Children { get; set; } = new List<ChildDTO>();
        public int IdParent { get; set; }
    }

    public class ChildDTO
    {
        public int IdChild { get; set; }
        public ParentDTO Parent { get; set; }
    }

    public class ParentModel
    {
        public ChildModel First { get; set; }
        public List<ChildModel> Children { get; set; } = new List<ChildModel>();
        public int IdParent { get; set; }
    }

    public class ChildModel
    {
        int _idChild;

        public ChildModel(ParentModel parent)
        {
            Parent = parent;
        }

        public int IdChild
        {
            get => _idChild;
            set
            {
                if(_idChild != 0)
                {
                    throw new Exception("Set IdChild again.");
                }
                _idChild = value;
            }
        }
        public ParentModel Parent { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ChildDTO, ChildModel>().ForMember(c => c.Parent, o => o.Ignore());
        cfg.CreateMap<ParentDTO, ParentModel>();
    });

    [Fact]
    public void Should_work()
    {
        var parentDto = new ParentDTO { IdParent = 1 };
        for(var i = 0; i < 5; i++)
        {
            parentDto.Children.Add(new ChildDTO { IdChild = i, Parent = parentDto });
        }

        var mappedChildren = Mapper.Map<List<ChildDTO>, List<ChildModel>>(parentDto.Children);
    }
}

public class Preserve_references_with_constructor_mapping : AutoMapperSpecBase
{
    public class ParentDTO
    {
        public ChildDTO First => Children[0];
        public List<ChildDTO> Children { get; set; } = new List<ChildDTO>();
        public int IdParent { get; set; }
    }

    public class ChildDTO
    {
        public int IdChild { get; set; }
        public ParentDTO Parent { get; set; }
    }

    public class ParentModel
    {
        public ChildModel First { get; set; }
        public List<ChildModel> Children { get; set; } = new List<ChildModel>();
        public int IdParent { get; set; }
    }

    public class ChildModel
    {
        int _idChild;

        public ChildModel(ParentModel parent)
        {
            Parent = parent;
        }

        public int IdChild
        {
            get => _idChild;
            set
            {
                if(_idChild != 0)
                {
                    throw new Exception("Set IdChild again.");
                }
                _idChild = value;
            }
        }
        public ParentModel Parent { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap<ParentDTO, ParentModel>().PreserveReferences();
        cfg.CreateMap<ChildDTO, ChildModel>().ForMember(c => c.Parent, o => o.Ignore()).PreserveReferences();
    });

    [Fact]
    public void Should_work()
    {
        var parentDto = new ParentDTO { IdParent = 1 };
        for(var i = 0; i < 5; i++)
        {
            parentDto.Children.Add(new ChildDTO { IdChild = i, Parent = parentDto });
        }

        var mappedChildren = Mapper.Map<List<ChildDTO>, List<ChildModel>>(parentDto.Children);
        var parentModel = mappedChildren.Select(c => c.Parent).Distinct().Single();
        parentModel.First.ShouldBe(mappedChildren[0]);
    }
}

public class When_construct_mapping_a_struct_with_string : AutoMapperSpecBase
{
    public struct Source
    {
        public string Property { get; set; }
    }

    public struct Destination
    {
        public Destination(string property)
        {
            Property = property;
        }

        public string Property { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_map_ok()
    {
        var source = new Source { Property = "value" };
        var destination = Mapper.Map<Destination>(source);
        destination.Property.ShouldBe("value");
    }
}

public class When_construct_mapping_a_struct : AutoMapperSpecBase
{
    public class Dto
    {
        public double Value { get; set; }
    }

    public class Entity
    {
        public double Value { get; set; }
    }

    public struct Source
    {
        public Dto Property { get; set; }
    }

    public struct Destination
    {
        public Destination(Entity property)
        {
            Property = property;
        }

        public Entity Property { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Dto, Entity>().ReverseMap();
        cfg.CreateMap<Source, Destination>().ReverseMap();
    });

    [Fact]
    public void Should_map_ok()
    {
        var source = new Source
        {
            Property = new Dto { Value = 5.0 }
        };
        var destination = Mapper.Map<Destination>(source);
        destination.Property.Value.ShouldBe(5.0);
        Mapper.Map<Source>(destination).Property.Value.ShouldBe(5.0);
    }
}

public class When_mapping_to_an_abstract_type : AutoMapperSpecBase
{
    class Source
    {
        public int Value { get; set; }
    }

    abstract class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c=>c.CreateMap<Source, Destination>());

    [Fact]
    public void Should_throw()
    {
        new Action(() => Mapper.Map<Destination>(new Source())).ShouldThrow<ArgumentException>($"Cannot create an instance of abstract type {typeof(Destination)}.");
    }
}

public class When_a_constructor_with_extra_parameters_doesnt_match : AutoMapperSpecBase
{
    PersonTarget _destination;

    class PersonSource
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    class PersonTarget
    {
        public int Age { get; set; }
        public string Name { get; set; }

        public PersonTarget(int age, string name)
        {
            this.Age = age;
            this.Name = name;
        }

        public PersonTarget(int age, string firstName, string lastName)
        {
            this.Age = age;
            this.Name = firstName + " " + lastName;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c=>c.CreateMap<PersonSource, PersonTarget>());

    protected override void Because_of()
    {
        _destination = Mapper.Map<PersonTarget>(new PersonSource { Age = 23, Name = "Marc" });
    }

    [Fact]
    public void We_should_choose_a_matching_constructor()
    {
        _destination.Age.ShouldBe(23);
        _destination.Name.ShouldBe("Marc");
    }
}

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

    protected override MapperConfiguration CreateConfiguration() => new(c =>
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
        _destination.InnerDestination.Name.ShouldBe("Core");
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _destination.Name.ShouldBe("John");
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _destination.Name.ShouldBe("John");
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Person, PersonDto>().ConstructUsing(p=>new PersonDto()));

    protected override void Because_of()
    {
        _destination = Mapper.Map<PersonDto>(new Person { Name = "John" });
    }

    [Fact]
    public void Should_map_from_the_property()
    {
        var typeMap = FindTypeMapFor<Person, PersonDto>();
        _destination.Name.ShouldBe("John");
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Person, PersonDto>());

    protected override void Because_of()
    {
        _destination = Mapper.Map<PersonDto>(new Person { Name = "John" });
    }

    [Fact]
    public void Should_map_from_the_property()
    {
        _destination.Name.ShouldBe("John");
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _destination.Latitude.ShouldBe(34);
        _destination.Longitude.ShouldBe(-93);
        _destination.HorizontalAccuracy.ShouldBe(100);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _destination.Latitude.ShouldBe(34);
        _destination.Longitude.ShouldBe(-93);
        _destination.HorizontalAccuracy.ShouldBe(100);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePostfixes("Id");
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<int, MyType>();
    });

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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePostfixes("Id");
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<int, MyType>();
    });

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

    protected override MapperConfiguration CreateConfiguration() => new(c=>c.CreateMap<Source, Destination>());

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source());
    }

    [Fact]
    public void Should_map_ok()
    {
        _destination.Id.ShouldBe(Guid.Empty);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _destination.Foo.ShouldBe(5);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _destination.Foo.ShouldBe(5);
        _destination.Bar.ShouldBe("bar");
    }
}

public class When_mapping_constructor_argument_fails : NonValidatingSpecBase
{
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

        public Dest(Dest foo)
        {
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>();
    });

    [Fact]
    public void Should_say_what_parameter_fails()
    {
        var ex = new Action(AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>();
        ex.Message.ShouldContain("Void .ctor(Dest), parameter foo", Case.Sensitive);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _dest.Foo.ShouldBe(5);
    }

    [Fact]
    public void Should_map_the_existing_properties()
    {
        _dest.Bar.ShouldBe(10);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _dest.Foo.ShouldBe(5);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _dest.Foo.ShouldBe(10);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _dest.Foo.ShouldBe(11);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
        _dest.Foo.ShouldBe(5);
        _dest.Bar.ShouldBe(10);
    }
}
public class When_mapping_with_optional_parameters_and_constructor_mapping_is_disabled : AutoMapperSpecBase
{
    public class Destination
    {
        public Destination(Destination destination = null)
        {
            Dest = destination;
        }
        public Destination Dest { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.DisableConstructorMapping();
        cfg.CreateMap<object, Destination>();
    });
    [Fact]
    public void Should_map_ok() => Mapper.Map<Destination>(new object()).Dest.ShouldBeNull();
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

        destinationFoo.Bar.FooBar.ShouldBe(sourceBar.FooBar);
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

        destinationFoo.Bar.FooBar.ShouldBe(sourceBar.FooBar);
        destinationFoo.Bar2.FooBar.ShouldBe("fooBar2");
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
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceBar, DestinationBar>();
            cfg.CreateMap<SourceFoo, DestinationFoo>();
        });

        var sourceBar = new SourceBar("fooBar");
        var sourceFoo = new SourceFoo(sourceBar);

        var destinationFoo = config.CreateMapper().Map<DestinationFoo>(sourceFoo);

        destinationFoo.Bar.FooBar.ShouldBe("fooBar");
        destinationFoo.Str.ShouldBe("hello");
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
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceBar, DestinationBar>();
            cfg.CreateMap<SourceFoo, DestinationFoo>();
        });

        var sourceBar = new SourceBar("fooBar");
        var sourceFoo = new SourceFoo(sourceBar);

        var destinationFoo = config.CreateMapper().Map<DestinationFoo>(sourceFoo);

        destinationFoo.Bar.FooBar.ShouldBe("fooBar");
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

        destinationFoo.A.ShouldBe("a");
        destinationFoo.B.ShouldBe("b");
        destinationFoo.C.ShouldBe(3);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt => opt.MapFrom(src => src.Value));
    });

    [Fact]
    public void Should_redirect_value()
    {
        var dest = Mapper.Map<Source, Dest>(new Source {Value = 5});

        dest.Value1.ShouldBe(5);
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt => opt.MapFrom(src => src.Value));
    });

    [Fact]
    public void Should_redirect_value()
    {
        var dest = Mapper.Map<Source, Dest>(new Source());

        dest.Value1.ShouldBeNull();
    }
}

public class When_configuring_ctor_param_members_without_source_property_1 : AutoMapperSpecBase
{
    public class Source
    {
        public string Result { get; }

        public Source(string result)
        {
            Result = result;
        }
    }

    public class Dest
    {
        public string Result{ get; }
        public dynamic Details { get; }

        public Dest(string result, DestInner1 inner1)
        {
            Result = result;
            Details = inner1;
        }
        public Dest(string result, DestInner2 inner2)
        {
            Result = result;
            Details = inner2;
        }

        public class DestInner1
        {
            public int Value { get; }

            public DestInner1(int value)
            {
                Value = value;
            }
        }

        public class DestInner2
        {
            public int Value { get; }

            public DestInner2(int value)
            {
                Value = value;
            }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(config =>
    {
        config.CreateMap<Source, Dest>()
            .ForCtorParam("inner1", cfg => cfg.MapFrom(_ => new Dest.DestInner1(100)));
    });

    [Fact]
    public void Should_redirect_value()
    {
        var dest = Mapper.Map<Dest>(new Source("Success"));

        dest.ShouldNotBeNull();
        Assert.Equal("100", dest.Details.Value.ToString());
    }
}

public class When_configuring_ctor_param_members_without_source_property_2 : AutoMapperSpecBase
{
    public class Source
    {
        public string Result { get; }

        public Source(string result)
        {
            Result = result;
        }
    }

    public class Dest
    {
        public string Result{ get; }
        public dynamic Details { get; }

        public Dest(string result, DestInner1 inner1)
        {
            Result = result;
            Details = inner1;
        }
        public Dest(string result, DestInner2 inner2)
        {
            Result = result;
            Details = inner2;
        }

        public class DestInner1
        {
            public int Value { get; }

            public DestInner1(int value)
            {
                Value = value;
            }
        }

        public class DestInner2
        {
            public int Value { get; }

            public DestInner2(int value)
            {
                Value = value;
            }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(config =>
    {
        config.CreateMap<Source, Dest>()
            .ForCtorParam("inner2", cfg => cfg.MapFrom(_ => new Dest.DestInner2(100)));
    });

    [Fact]
    public void Should_redirect_value()
    {
        var dest = Mapper.Map<Dest>(new Source("Success"));

        dest.ShouldNotBeNull();
        Assert.Equal("100", dest.Details.Value.ToString());
    }
}

