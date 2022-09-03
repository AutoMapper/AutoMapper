namespace AutoMapper.UnitTests.Bug;

public class GenericTypeConverterWithTwoArguments : AutoMapperSpecBase
{
    List<object> _destination;

    protected override MapperConfiguration CreateConfiguration() => new(c=>c.CreateMap(typeof(List<>), typeof(List<>)).ConvertUsing(typeof(Converter<,>)));

    protected override void Because_of()
    {
        _destination = Mapper.Map<List<int>, List<object>>(Enumerable.Range(1, 10).ToList());
    }

    [Fact]
    public void Should_work()
    {
        _destination.ShouldBe(Converter<int, object>.Result);
    }

    public class Converter<TSource, TDestination> : ITypeConverter<List<TSource>, List<TDestination>>
    {
        public static readonly List<TDestination> Result = new List<TDestination>();

        public List<TDestination> Convert(List<TSource> source, List<TDestination> destination, ResolutionContext context)
        {
            return Result;
        }
    }
}

public class GenericTypeConverter : AutoMapperSpecBase
{
    Destination<int> _destination;
    OtherDestination<int> _otherDestination;
    int _openGenericToNonGenericDestination;
    Destination<int> _nonGenericToOpenGenericDestination;
    OtherDestination<int> _closedGenericToOpenGenericDestination;
    Destination<object> _openGenericToClosedGenericDestination;

    public class Source<T>
    {
        public T Value { get; set; }
    }

    public class Destination<T>
    {
        public T Value { get; set; }
    }

    public class OtherSource<T>
    {
        public T Value { get; set; }
    }

    public class OtherDestination<T>
    {
        public T Value { get; set; }
    }

    public class Converter<T> :
        ITypeConverter<Source<T>, Destination<T>>,
        ITypeConverter<OtherSource<T>, OtherDestination<T>>,
        ITypeConverter<Source<T>, int>,
        ITypeConverter<int, Destination<T>>,
        ITypeConverter<OtherSource<T>, Destination<object>>,
        ITypeConverter<Source<object>, OtherDestination<T>>
    {
        public static Destination<T> SomeDestination = new Destination<T>();
        public static OtherDestination<T> SomeOtherDestination = new OtherDestination<T>();
        public static int NongenericDestination = default(int);
        public static OtherDestination<T> OpenDestinationViaClosedSource = new OtherDestination<T>();
        public static Destination<object> ClosedDestinationViaOpenSource = new Destination<object>();

        public Destination<T> Convert(Source<T> source, Destination<T> dest, ResolutionContext context)
        {
            return SomeDestination;
        }

        OtherDestination<T> ITypeConverter<OtherSource<T>, OtherDestination<T>>.Convert(OtherSource<T> source, OtherDestination<T> dest, ResolutionContext context)
        {
            return SomeOtherDestination;
        }

        int ITypeConverter<Source<T>, int>.Convert(Source<T> source, int dest, ResolutionContext context)
        {
            return NongenericDestination;
        }

        Destination<T> ITypeConverter<int, Destination<T>>.Convert(int source, Destination<T> dest, ResolutionContext context)
        {
            return SomeDestination;
        }

        Destination<object> ITypeConverter<OtherSource<T>, Destination<object>>.Convert(OtherSource<T> source, Destination<object> dest, ResolutionContext context)
        {
            return ClosedDestinationViaOpenSource;
        }

        OtherDestination<T> ITypeConverter<Source<object>, OtherDestination<T>>.Convert(Source<object> source, OtherDestination<T> dest, ResolutionContext context)
        {
            return OpenDestinationViaClosedSource;
        }
    }

    public class Converter<T1, T2> : ITypeConverter<Hashtable, IReadOnlyDictionary<T1, T2>>

    {
        public static IReadOnlyDictionary<T1, T2> ReadOnlyDictionaryDestination = new Dictionary<T1, T2>();

        public IReadOnlyDictionary<T1, T2> Convert(Hashtable source, IReadOnlyDictionary<T1, T2> dest, ResolutionContext context)
        {
            return ReadOnlyDictionaryDestination;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof (Source<>), typeof (Destination<>)).ConvertUsing(typeof (Converter<>));
        cfg.CreateMap(typeof (OtherSource<>), typeof (OtherDestination<>)).ConvertUsing(typeof (Converter<>));
        cfg.CreateMap(typeof (Source<>), typeof (int)).ConvertUsing(typeof (Converter<>));
        cfg.CreateMap(typeof (int), typeof (Destination<>)).ConvertUsing(typeof (Converter<>));
        cfg.CreateMap(typeof (OtherSource<>), typeof (Destination<object>)).ConvertUsing(typeof (Converter<>));
        cfg.CreateMap(typeof(Source<object>), typeof(OtherDestination<>)).ConvertUsing(typeof(Converter<>));
        cfg.CreateMap(typeof (Hashtable), typeof (IReadOnlyDictionary<,>)).ConvertUsing(typeof (Converter<,>));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination<int>>(new Source<int>());
        _otherDestination = Mapper.Map<OtherDestination<int>>(new OtherSource<int>());
        _openGenericToNonGenericDestination = Mapper.Map<int>(new Source<int>());
        _nonGenericToOpenGenericDestination = Mapper.Map<Destination<int>>(default(int));
        _openGenericToClosedGenericDestination = Mapper.Map<Destination<object>>(new OtherSource<int>());
        _closedGenericToOpenGenericDestination = Mapper.Map<OtherDestination<int>>(new Source<object>());
    }

    [Fact]
    public void Should_use_generic_converter_with_correct_interface()
    {
        _destination.ShouldBeSameAs(Converter<int>.SomeDestination);
        _otherDestination.ShouldBeSameAs(Converter<int>.SomeOtherDestination);
        _openGenericToNonGenericDestination.ShouldBe(Converter<int>.NongenericDestination);
        _nonGenericToOpenGenericDestination.ShouldBeSameAs(Converter<int>.SomeDestination);
        _openGenericToClosedGenericDestination.ShouldBe(Converter<int>.ClosedDestinationViaOpenSource);
        _closedGenericToOpenGenericDestination.ShouldBe(Converter<int>.OpenDestinationViaClosedSource);
    }

    [Fact]
    public void Should_use_generic_converter_when_covered_by_object_map()
    {
        Mapper.Map<IReadOnlyDictionary<int, int>>(new Hashtable()).ShouldBeSameAs(Converter<int, int>.ReadOnlyDictionaryDestination);
    }

    [Fact]
    public void Should_use_generic_converter_with_correct_closed_type()
    {
        Mapper.Map<Destination<int>>(new Source<int>()).ShouldBeSameAs(Converter<int>.SomeDestination);
        Mapper.Map<Destination<long>>(new Source<long>()).ShouldBeSameAs(Converter<long>.SomeDestination);
    }
}
