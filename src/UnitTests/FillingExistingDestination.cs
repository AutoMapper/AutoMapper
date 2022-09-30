namespace AutoMapper.UnitTests;

public class When_a_source_child_object_is_null : AutoMapperSpecBase
{
    public class Source
    {
        public Child Child { get; set; }
    }

    public class Destination
    {
        public Child Child { get; set; } = new Child();
    }

    public class Child
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForMember(d => d.Child, o => o.MapAtRuntime());
    });

    [Fact]
    public void Should_overwrite_the_existing_child_destination()
    {
        var destination = new Destination();
        Mapper.Map(new Source(), destination);
        destination.Child.ShouldBeNull();
    }
}

public class When_the_destination_object_is_specified : AutoMapperSpecBase
{
    private Source _source;
    private Destination _originalDest;
    private Destination _dest;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();

    });

    protected override void Because_of()
    {
        _source = new Source
        {
            Value = 10,
        };
        _originalDest = new Destination { Value = 1111 };
        _dest = Mapper.Map<Source, Destination>(_source, _originalDest);
    }

    [Fact]
    public void Should_do_the_translation()
    {
        _dest.Value.ShouldBe(10);
    }

    [Fact]
    public void Should_return_the_destination_object_that_was_passed_in()
    {
        _originalDest.ShouldBeSameAs(_dest);
    }
}
   
public class When_the_destination_object_is_specified_with_child_objects : AutoMapperSpecBase
{
    private Source _source;
    private Destination _originalDest;
    private Destination _dest;

    public class Source
    {
        public int Value { get; set; }
        public ChildSource Child { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public ChildDestination Child { get; set; }
    }

    public class ChildSource
    {
        public int Value { get; set; }
    }

    public class ChildDestination
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>(MemberList.Source)
            .ForMember(d => d.Child, opt => opt.UseDestinationValue());
        cfg.CreateMap<ChildSource, ChildDestination>(MemberList.Source)
            .ForMember(d => d.Name, opt => opt.UseDestinationValue());
    });

    protected override void Because_of()
    {
        _source = new Source
        {
            Value = 10,
            Child = new ChildSource
            {
                Value = 20
            }
        };
        _originalDest = new Destination
        {
            Value = 1111,
            Name = "foo",
            Child = new ChildDestination
            {
                Name = "bar"
            }
        };
        _dest = Mapper.Map<Source, Destination>(_source, _originalDest);
    }

    [Fact]
    public void Should_do_the_translation()
    {
        _dest.Value.ShouldBe(10);
        _dest.Child.Value.ShouldBe(20);
    }

    [Fact]
    public void Should_return_the_destination_object_that_was_passed_in()
    {
        _dest.Name.ShouldBe("foo");
        _dest.Child.Name.ShouldBe("bar");
    }
}

public class When_the_destination_object_has_child_objects : AutoMapperSpecBase
{
    private Source _source;
    private Destination _originalDest;
    private ChildDestination _originalDestChild;
    private Destination _dest;

    public class Source
    {
        public ChildSource Child { get; set; }
    }

    public class Destination
    {
        public ChildDestination Child { get; set; }
    }

    public class ChildSource
    {
        public int Value { get; set; }
    }

    public class ChildDestination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<ChildSource, ChildDestination>();
    });

    protected override void Because_of()
    {
        _source = new Source
        {
            Child = new ChildSource
            {
                Value = 20
            }
        };
        _originalDestChild = new ChildDestination
        {
            Value = 10
        };
        _originalDest = new Destination
        {
            Child = _originalDestChild
        };
        _dest = Mapper.Map(_source, _originalDest);
    }

    [Fact]
    public void Should_return_the_destination_object_that_was_passed_in()
    {
        _dest.ShouldBeSameAs(_originalDest);
        _dest.Child.ShouldBeSameAs(_originalDestChild);
        _dest.Child.Value.ShouldBe(20);
    }
}


public class When_the_destination_object_is_specified_and_you_are_converting_an_enum : NonValidatingSpecBase
{
    private string _result;

    public enum SomeEnum
    {
        One,
        Two,
        Three
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });


    protected override void Because_of()
    {
        _result = Mapper.Map<SomeEnum, string>(SomeEnum.Two, "test");
    }

    [Fact]
    public void Should_return_the_enum_as_a_string()
    {
        _result.ShouldBe("Two");
    }
}