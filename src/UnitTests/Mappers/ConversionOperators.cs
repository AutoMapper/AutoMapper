namespace AutoMapper.UnitTests;

public class Nullable_conversion_operator : NonValidatingSpecBase
{
    public class QueryableValue<T>
    {
        public T Value { get; set; }
        public static implicit operator QueryableValue<T>(T obj) => new() { Value = obj };
        public static implicit operator T(QueryableValue<T> obj) => obj.Value;
    }
    class Destination
    {
        public QueryableValue<int?> MyProperty { get; set; } = null!;
    }
    class Source
    {
        public int? MyProperty { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<Source, Destination>());
    [Fact]
    public void Should_work() => Map<Destination>(new Source { MyProperty = 42 }).MyProperty.Value.ShouldBe(42);
}
public class When_mapping_to_classes_with_implicit_conversion_operators_on_the_destination
{
    private Bar _bar;

    public class Foo
    {
        public string Value { get; set; }
    }

    public class Bar
    {
        public string OtherValue { get; set; }

        public static implicit operator Bar(Foo other)
        {
            return new Bar
            {
                OtherValue = other.Value
            };
        }

    }

    [Fact]
    public void Should_use_the_implicit_conversion_operator()
    {
        var source = new Foo { Value = "Hello" };
        var config = new MapperConfiguration(cfg => { });

        _bar = config.CreateMapper().Map<Foo, Bar>(source);

        _bar.OtherValue.ShouldBe("Hello");
    }
}
    
public class When_mapping_to_classes_with_implicit_conversion_operators_on_the_source
{
    private Bar _bar;

    public class Foo
    {
        public string Value { get; set; }

        public static implicit operator Bar(Foo other)
        {
            return new Bar
            {
                OtherValue = other.Value
            };
        }

        public static implicit operator string(Foo other)
        {
            return other.Value;
        }

    }

    public class InheritedFoo : Foo
    { }

    public class Bar
    {
        public string OtherValue { get; set; }
    }

    [Fact]
    public void Should_use_the_implicit_conversion_operator()
    {
        var source = new Foo { Value = "Hello" };

        var config = new MapperConfiguration(cfg => { });
        _bar = config.CreateMapper().Map<Foo, Bar>(source);

        _bar.OtherValue.ShouldBe("Hello");
    }

    [Fact]
    public void Should_use_the_inherited_implicit_conversion_operator()
    {
        var source = new InheritedFoo { Value = "Hello" };

        var config = new MapperConfiguration(cfg => { });
        _bar = config.CreateMapper().Map<InheritedFoo, Bar>(source);

        _bar.OtherValue.ShouldBe("Hello");
    }
}

public class When_mapping_to_classes_with_explicit_conversion_operator_on_the_destination
{
    private Bar _bar;

    public class Foo
    {
        public string Value { get; set; }
    }

    public class Bar
    {
        public string OtherValue { get; set; }

        public static explicit operator Bar(Foo other)
        {
            return new Bar
            {
                OtherValue = other.Value
            };
        }
    }

    [Fact]
    public void Should_use_the_explicit_conversion_operator()
    {
        var config = new MapperConfiguration(cfg => { });
        _bar = config.CreateMapper().Map<Foo, Bar>(new Foo { Value = "Hello" });
        _bar.OtherValue.ShouldBe("Hello");
    }
}

public class When_mapping_to_classes_with_explicit_conversion_operator_on_the_source
{
    private Bar _bar;

    public class Foo
    {
        public string Value { get; set; }

        public static explicit operator Bar(Foo other)
        {
            return new Bar
            {
                OtherValue = other.Value
            };
        }
    }

    public class InheritedFoo : Foo
    { }

    public class Bar
    {
        public string OtherValue { get; set; }
    }

    [Fact]
    public void Should_use_the_explicit_conversion_operator()
    {
        var config = new MapperConfiguration(cfg => { });
        _bar = config.CreateMapper().Map<Foo, Bar>(new Foo { Value = "Hello" });
        _bar.OtherValue.ShouldBe("Hello");
    }

    [Fact]
    public void Should_use_the_inherited_explicit_conversion_operator()
    {
        var source = new InheritedFoo { Value = "Hello" };

        var config = new MapperConfiguration(cfg => { });
        _bar = config.CreateMapper().Map<InheritedFoo, Bar>(source);

        _bar.OtherValue.ShouldBe("Hello");
    }
}
