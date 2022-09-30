namespace AutoMapper.UnitTests;

public class ShouldIgnoreOpenGenericMethods : NonValidatingSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<Source, Destination>());
    class Source
    {
        public int GetValue<T>() => 42;
    }
    class Destination
    {
        public int Value { get; set; }
    }
    [Fact]
    public void Works() => Map<Destination>(new Source()).Value.ShouldBe(0);
}
public class ShouldMapMethodInstanceMethods : NonValidatingSpecBase
{
    public int SomeValue = 2354;
    public int AnotherValue = 6798;

    private Destination _destination;

    class Source
    {
        private int _someValue;
        private int _anotherValue;

        public Source(int someValue, int anotherValue) 
        {
            _someValue = someValue;
            anotherValue = _anotherValue;
        }

        public int SomeNumber() 
        {
            return _someValue;
        }

        public int AnotherNumber() {
            return _anotherValue;
        }
    }

    class Destination
    {
        public int SomeNumber { get; set; }
        public int AnotherNumber { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.ShouldMapMethod = (m => m.Name != nameof(Source.AnotherNumber));
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source(SomeValue, AnotherValue));
    }

    [Fact]
    public void Should_report_unmapped_property()
    {
        new Action(AssertConfigurationIsValid)
            .ShouldThrowException<AutoMapperConfigurationException>(ex => 
            {
                ex.Errors.ShouldNotBeNull();
                ex.Errors.ShouldNotBeEmpty();
                ex.Errors[0].UnmappedPropertyNames.ShouldNotBeNull();
                ex.Errors[0].UnmappedPropertyNames.ShouldNotBeEmpty();
                ex.Errors[0].UnmappedPropertyNames[0].ShouldBe(nameof(Destination.AnotherNumber));
            });
    }

    [Fact]
    public void Should_not_map_another_number_method() 
    {
        _destination.SomeNumber.ShouldBe(SomeValue);
        _destination.AnotherNumber.ShouldNotBe(AnotherValue);
    }
}


static class SourceExtensions 
{
    public static int SomeNumber(this ShouldMapMethodExtensionMethods.Source source) 
    {
        return source.SomeValue;
    }

    public static int AnotherNumber(this ShouldMapMethodExtensionMethods.Source source) 
    {
        return source.AnotherValue;
    }
}

public class ShouldMapMethodExtensionMethods : NonValidatingSpecBase 
{
    public int SomeValue = 4698;
    public int AnotherValue = 2374;

    private Destination _destination;

    public class Source 
    {
        public int SomeValue { get; set; }
        public int AnotherValue { get; set; }
    }

    public class Destination 
    {
        public int SomeNumber { get; set; }
        public int AnotherNumber { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => 
    {
        cfg.IncludeSourceExtensionMethods(typeof(SourceExtensions));
        cfg.ShouldMapMethod = (m => m.Name != nameof(SourceExtensions.AnotherNumber));
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of() 
    {
        _destination = Mapper.Map<Source, Destination>(new Source { SomeValue = SomeValue, AnotherValue = AnotherValue });
    }

    [Fact]
    public void Should_report_unmapped_property() 
    {
        new Action(AssertConfigurationIsValid)
            .ShouldThrowException<AutoMapperConfigurationException>(ex => 
            {
                ex.Errors.ShouldNotBeNull();
                ex.Errors.ShouldNotBeEmpty();
                ex.Errors[0].UnmappedPropertyNames.ShouldNotBeNull();
                ex.Errors[0].UnmappedPropertyNames.ShouldNotBeEmpty();
                ex.Errors[0].UnmappedPropertyNames[0].ShouldBe(nameof(Destination.AnotherNumber));
            });
    }

    [Fact]
    public void Should_not_map_another_number_method() 
    {
        _destination.SomeNumber.ShouldBe(SomeValue);
        _destination.AnotherNumber.ShouldNotBe(AnotherValue);
    }
}