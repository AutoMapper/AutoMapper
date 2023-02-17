namespace AutoMapper.UnitTests.CustomMapping;
public class StringToEnumConverter : AutoMapperSpecBase
{
    class Source
    {
        public string Enum { get; set; }
    }
    class Destination
    {
        public ConsoleColor Enum { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => 
    { 
        c.CreateMap<string, Enum>().ConvertUsing(s => ConsoleColor.DarkCyan);
        c.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Should_work()
    {
        Map<ConsoleColor>("").ShouldBe(ConsoleColor.DarkCyan);
        Map<Destination>(new Source()).Enum.ShouldBe(ConsoleColor.DarkCyan);
    }
}
public class NullableConverter : AutoMapperSpecBase
{
    public enum GreekLetters
    {
        Alpha = 11,
        Beta = 12,
        Gamma = 13
    }

    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<int?, GreekLetters>().ConvertUsing(n => n == null ? GreekLetters.Beta : GreekLetters.Gamma);
    });

    [Fact]
    public void Should_map_nullable()
    {
        Mapper.Map<int?, GreekLetters>(null).ShouldBe(GreekLetters.Beta);
        Mapper.Map<int?, GreekLetters>(42).ShouldBe(GreekLetters.Gamma);
    }
}

public class MissingConverter : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.ConstructServicesUsing(t => null);
        c.CreateMap<int, int>().ConvertUsing<ITypeConverter<int, int>>();
    });

    [Fact]
    public void Should_report_the_missing_converter()
    {
        new Action(()=>Mapper.Map<int, int>(0))
            .ShouldThrowException<AutoMapperMappingException>(e=>e.Message.ShouldBe("Cannot create an instance of type AutoMapper.ITypeConverter`2[System.Int32,System.Int32]"));
    }
}

public class DecimalAndNullableDecimal : AutoMapperSpecBase
{
    Destination _destination;

    class Source
    {
        public decimal Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public decimal? Value3 { get; set; }
    }

    class Destination
    {
        public decimal? Value1 { get; set; }
        public decimal Value2 { get; set; }
        public decimal? Value3 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<decimal?, decimal>().ConvertUsing(source => source ?? decimal.MaxValue);
        cfg.CreateMap<decimal, decimal?>().ConvertUsing(source => source == decimal.MaxValue ? new decimal?() : source);
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { Value1 = decimal.MaxValue });
    }


    [Fact]
    public void Should_treat_max_value_as_null()
    {
        _destination.Value1.ShouldBeNull();
        _destination.Value2.ShouldBe(decimal.MaxValue);
        _destination.Value3.ShouldBeNull();
    }
}

public class When_converting_to_string : AutoMapperSpecBase
{
    Destination _destination;

    class Source
    {
        public Id TheId { get; set; }
    }

    class Destination
    {
        public string TheId { get; set; }
    }

    interface IId
    {
        string Serialize();
    }

    class Id : IId
    {
        public string Prefix { get; set; }

        public string Value { get; set; }

        public string Serialize()
        {
            return Prefix + "_" + Value;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<IId, string>().ConvertUsing(id => id.Serialize());
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { TheId = new Id { Prefix = "p", Value = "v" } });
    }

    [Fact]
    public void Should_use_the_type_converter()
    {
        _destination.TheId.ShouldBe("p_v");
    }
}

public class When_specifying_type_converters_for_object_mapper_types : AutoMapperSpecBase
{
    class Source
    {
        public IDictionary<int, int> Values { get; set; }
    }
    class Destination
    {
        public IDictionary<int, int> Values { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(IDictionary<,>), typeof(IDictionary<,>)).ConvertUsing(typeof(DictionaryConverter<,>));
        cfg.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Should_override_the_built_in_mapper()
    {
        var destination = Mapper.Map<Destination>(new Source { Values = new Dictionary<int, int>() });
        destination.Values.ShouldBeSameAs(DictionaryConverter<int, int>.Instance);
        var destinationString = Mapper.Map<Dictionary<string, string>>(new Dictionary<string, string>());
        destinationString.ShouldBeSameAs(DictionaryConverter<string, string>.Instance);
    }
    private class DictionaryConverter<TKey, TValue> : ITypeConverter<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>>
    {
        public static readonly IDictionary<TKey, TValue> Instance = new Dictionary<TKey, TValue>();
        public IDictionary<TKey, TValue> Convert(IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> destination, ResolutionContext context) => Instance;
    }
}

public class When_specifying_type_converters : AutoMapperSpecBase
{
    private Destination _result;

    public class Source
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
    }

    public class Destination
    {
        public int Value1 { get; set; }
        public DateTime Value2 { get; set; }
        public Type Value3 { get; set; }
    }

    public class DateTimeTypeConverter : ITypeConverter<string, DateTime>
    {
        public DateTime Convert(string source, DateTime destination, ResolutionContext context)
        {
            return System.Convert.ToDateTime(source);
        }
    }

    public class TypeTypeConverter : ITypeConverter<string, Type>
    {
        public Type Convert(string source, Type destination, ResolutionContext context)
        {
            Type type = typeof(TypeTypeConverter).Assembly.GetType(source);
            return type;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<string, int>().ConvertUsing((string arg) => System.Convert.ToInt32(arg));
        cfg.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
        cfg.CreateMap<string, Type>().ConvertUsing<TypeTypeConverter>();
        cfg.CreateMap<Source, Destination>();

    });

    protected override void Because_of()
    {
        var source = new Source
        {
            Value1 = "5",
            Value2 = "01/01/2000",
            Value3 = "AutoMapper.UnitTests.CustomMapping.When_specifying_type_converters+Destination"
        };

        _result = Mapper.Map<Source, Destination>(source);
    }

    [Fact]
    public void Should_convert_type_using_expression()
    {
        _result.Value1.ShouldBe(5);
    }

    [Fact]
    public void Should_convert_type_using_instance()
    {
        _result.Value2.ShouldBe(new DateTime(2000, 1, 1));
    }

    [Fact]
    public void Should_convert_type_using_Func_that_returns_instance()
    {
        _result.Value3.ShouldBe(typeof(Destination));
    }
}

public class When_specifying_type_converters_on_types_with_incompatible_members : AutoMapperSpecBase
{
    private ParentDestination _result;

    public class Source
    {
        public string Foo { get; set; }
    }

    public class Destination
    {
        public int Type { get; set; }
    }

    public class ParentSource
    {
        public Source Value { get; set; }
    }

    public class ParentDestination
    {
        public Destination Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ConvertUsing(arg => new Destination {Type = System.Convert.ToInt32(arg.Foo)});
        cfg.CreateMap<ParentSource, ParentDestination>();

    });

    protected override void Because_of()
    {
        var source = new ParentSource
        {
            Value = new Source { Foo = "5", }
        };

        _result = Mapper.Map<ParentSource, ParentDestination>(source);
    }

    [Fact]
    public void Should_convert_type_using_expression()
    {
        _result.Value.Type.ShouldBe(5);
    }
}
public class When_specifying_a_type_converter_for_a_non_generic_configuration : NonValidatingSpecBase
{
    private Destination _result;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int OtherValue { get; set; }
    }

    public class CustomConverter : ITypeConverter<Source, Destination>
    {
        public Destination Convert(Source source, Destination destination, ResolutionContext context)
        {
            return new Destination
                {
                    OtherValue = source.Value + 10
                };
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ConvertUsing<CustomConverter>();
    });

    protected override void Because_of()
    {
        _result = Mapper.Map<Source, Destination>(new Source {Value = 5});
    }

    [Fact]
    public void Should_use_converter_specified()
    {
        _result.OtherValue.ShouldBe(15);
    }
}

public class When_specifying_a_non_generic_type_converter_for_a_non_generic_configuration : AutoMapperSpecBase
{
    private Destination _result;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int OtherValue { get; set; }
    }

    public class CustomConverter : ITypeConverter<Source, Destination>
    {
        public Destination Convert(Source source, Destination destination, ResolutionContext context)
        {
            return new Destination
                {
                    OtherValue = source.Value + 10
                };
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof (Source), typeof (Destination)).ConvertUsing(typeof (CustomConverter));
    });

    protected override void Because_of()
    {
        _result = Mapper.Map<Source, Destination>(new Source {Value = 5});
    }

    [Fact]
    public void Should_use_converter_specified()
    {
        _result.OtherValue.ShouldBe(15);
    }
}