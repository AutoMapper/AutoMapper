using StringDictionary = System.Collections.Generic.Dictionary<string, object>;

namespace AutoMapper.UnitTests.Mappers;

class Destination
{
    public string Foo { get; set; }
    public string Bar { get; set; }
    public int Baz { get; set; }
    public InnerDestination Inner { get; } = new InnerDestination();
    public InnerDestination NullInner { get; }
    public InnerDestination SettableInner { get; set; }
}
class InnerDestination
{
    public int Value { get; set; }
    public InnerDestination Child { get; set; }
}
public class When_mapping_to_StringDictionary : NonValidatingSpecBase
{
    StringDictionary _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        _destination = Mapper.Map<StringDictionary>(new Destination { Foo = "Foo", Bar = "Bar" });
    }

    [Fact]
    public void Should_map_source_properties()
    {
        _destination["Foo"].ShouldBe("Foo");
        _destination["Bar"].ShouldBe("Bar");
    }
    [Fact]
    public void Should_map_struct() => Map<StringDictionary>(new KeyValuePair<int, string>(1, "one")).ShouldBe(new StringDictionary { { "Key", 1 }, {"Value", "one"} });
}
public class When_mapping_from_StringDictionary : NonValidatingSpecBase
{
    Destination _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" } };
        _destination = Mapper.Map<Destination>(source);
    }

    [Fact]
    public void Should_map_destination_properties()
    {
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBe("Bar");
        _destination.Baz.ShouldBe(0);
    }
    [Fact]
    public void When_mapping_inner_properties()
    {
        var source = new StringDictionary() { { "Inner.Value", "5" }, { "NullInner.Value", "5" }, { "SettableInner.Value", "6" }, { "SettableInner.Child.Value", "7" },
            { "Inner.Child.Value", "8" }};
        var destination = Mapper.Map<Destination>(source);
        destination.Inner.Value.ShouldBe(5);
        destination.NullInner.ShouldBeNull();
        destination.SettableInner.Value.ShouldBe(6);
        destination.SettableInner.Child.Value.ShouldBe(7);
        destination.Inner.Child.Value.ShouldBe(8);
    }
}

public class When_mapping_struct_from_StringDictionary : NonValidatingSpecBase
{
    Destination _destination;

    struct Destination
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" } };
        _destination = Mapper.Map<Destination>(source);
    }

    [Fact]
    public void Should_map_destination_properties()
    {
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBe("Bar");
    }
    [Fact]
    public void Should_map_non_generic()
    {
        var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" } };
        var destination = (Destination) Mapper.Map(source, null, typeof(Destination));
        destination.Foo.ShouldBe("Foo");
        destination.Bar.ShouldBe("Bar");
    }
}

public class When_mapping_from_StringDictionary_with_missing_property : NonValidatingSpecBase
{
    Destination _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        var source = new StringDictionary() { { "Foo", "Foo" } };
        _destination = Mapper.Map<Destination>(source);
    }

    [Fact]
    public void Should_map_existing_properties()
    {
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBeNull();
        _destination.Baz.ShouldBe(0);
    }
}

public class When_mapping_from_StringDictionary_null_to_int : NonValidatingSpecBase
{
    Destination _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        var source = new StringDictionary() { { "Foo", "Foo" }, { "Baz", null } };
        _destination = Mapper.Map<Destination>(source);
    }

    [Fact]
    public void Should_map_to_zero()
    {
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBeNull();
        _destination.Baz.ShouldBe(0);
    }
}

public class When_mapping_from_StringDictionary_with_whitespace : NonValidatingSpecBase
{
    Destination _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        var source = new StringDictionary() { { " Foo", "Foo" }, { " Bar", "Bar" }, { " Baz ", 2 } };
        _destination = Mapper.Map<Destination>(source);
    }

    [Fact]
    public void Should_map()
    {
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBe("Bar");
        _destination.Baz.ShouldBe(2);
    }
}

public class When_mapping_from_StringDictionary_multiple_matching_keys : NonValidatingSpecBase
{
    StringDictionary _source;

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        _source = new StringDictionary() { { " Foo", "Foo1" }, { "  Foo", "Foo2" }, { "Bar", "Bar" }, { "Baz", 2 } };
    }

    [Fact]
    public void Should_throw_when_mapping()
    {
        Should.Throw<AutoMapperMappingException>(() =>
        {
            Mapper.Map<Destination>(_source);
        }).InnerException.ShouldBeOfType<AutoMapperMappingException>().Types.ShouldBe(new TypePair(typeof(IDictionary<string, object>), typeof(Destination)));
    }
}

public class When_mapping_from_StringDictionary_to_StringDictionary : NonValidatingSpecBase
{
    StringDictionary _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg => { });

    protected override void Because_of()
    {
        var source = new StringDictionary() { { "Foo", "Foo" }, { "Bar", "Bar" }, { "Bar ", "Bar_" } };
        _destination = Mapper.Map<StringDictionary>(source);
    }

    [Fact]
    public void Should_map()
    {
        _destination["Foo"].ShouldBe("Foo");
        _destination["Bar"].ShouldBe("Bar");
        _destination["Bar "].ShouldBe("Bar_");
    }
}

public class When_mapping_from_StringDictionary_to_existing_destination : AutoMapperSpecBase
{
    public abstract class SomeBase
    {
        protected int _x = 100;
        public abstract int X { get; }
        protected int _y = 200;
        public abstract int Y { get; }
    }

    public class SomeBody : SomeBase
    {
        public override int X { get { return _x + 10; } }

        public override int Y { get { return _y + 20; } }
        private int _z = 300;
        public int Z { get { return _z + 30; } }
        public int Value { get; set; }
        public Collection<int> Integers { get; } = new();
    }

    public class SomeOne : SomeBase
    {
        public override int X { get { return _x - 10; } }

        public override int Y { get { return _y - 20; } }
        private int _a = 300;
        public int A { get { return _a - 30; } }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<SomeBase, SomeBase>());

    [Fact]
    public void Should_map_ok()
    {
        var someBase = new SomeBody();
        var someOne = new StringDictionary { ["Integers"] = Enumerable.Range(1, 10).ToArray() };

        Mapper.Map(someOne, someBase);

        someBase.Integers.ShouldBe(someOne["Integers"]);
    }

    public class Destination
    {
        public DateTime? NullableDate { get; set; }
        public int? NullableInt { get; set; }
        public int Int { get; set; }
        public SomeBody SomeBody { get; set; } = new SomeBody { Value = 15 };
        public SomeOne SomeOne { get; set; } = new SomeOne();
        public string String { get; set; } = "value";
    }

    [Fact]
    public void Should_override_existing_values()
    {
        var source = new StringDictionary();
        source["Int"] = 10;
        source["NullableDate"] = null;
        source["NullableInt"] = null;
        source["String"] = null;
        source["SomeBody"] = new SomeOne();
        source["SomeOne"] = null;
        var destination = new Destination { NullableInt = 1, NullableDate = DateTime.Now };
        var someBody = destination.SomeBody;

        Mapper.Map(source, destination);

        destination.Int.ShouldBe(10);
        destination.NullableInt.ShouldBeNull();
        destination.NullableDate.ShouldBeNull();
        destination.SomeBody.ShouldBe(someBody);
        destination.SomeBody.Value.ShouldBe(15);
        destination.String.ShouldBeNull();
        destination.SomeOne.ShouldBeNull();
    }
}

public class When_mapping_from_StringDictionary_to_abstract_type : AutoMapperSpecBase
{
    public abstract class SomeBase
    {
        protected int _x = 100;
        public abstract int X { get; }
        protected int _y = 200;
        public abstract int Y { get; }
    }

    public class SomeBody : SomeBase
    {
        public override int X { get { return _x + 10; } }

        public override int Y { get { return _y + 20; } }
        private int _z = 300;
        public int Z { get { return _z + 30; } }
    }

    public class SomeOne : SomeBase
    {
        public override int X { get { return _x - 10; } }

        public override int Y { get { return _y - 20; } }
        private int _a = 300;
        public int A { get { return _a - 30; } }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<SomeBase, SomeBase>());

    [Fact]
    public void Should_throw()
    {
        new Action(() => Mapper.Map<SomeBase>(new StringDictionary()))
            .ShouldThrowException<AutoMapperMappingException>(ex =>
                ex.InnerException.Message.ShouldStartWith($"Cannot create an instance of abstract type {typeof(SomeBase)}."));
    }
}