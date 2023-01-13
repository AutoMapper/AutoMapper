namespace AutoMapper.UnitTests;
public class ForPathGenericsSource : AutoMapperSpecBase
{
    class Source<T>
    {
        public InnerSource Inner;
    }
    class InnerSource
    {
        public int Id;
    }
    class Destination
    {
        public int InnerId;
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap(typeof(Source<>), typeof(Destination)).ReverseMap());
    [Fact]
    public void Should_work() => Map<Destination>(new Source<int> { Inner = new() { Id = 42 } }).InnerId.ShouldBe(42);
}
public class ForPathGenerics : AutoMapperSpecBase
{
    class Source<T>
    {
        public InnerSource Inner;
    }
    class InnerSource
    {
        public int Id;
    }
    class Destination<T>
    {
        public int InnerId;
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap(typeof(Source<>), typeof(Destination<>)).ReverseMap());
    [Fact]
    public void Should_work() => Map<Destination<int>>(new Source<int> { Inner = new() { Id = 42 } }).InnerId.ShouldBe(42);
}
public class ReadonlyPropertiesGenerics : AutoMapperSpecBase
{
    class Source
    {
        public InnerSource Inner;
    }
    class InnerSource
    {
        public int Value;
    }
    class Destination<T>
    {
        public readonly InnerDestination Inner = new();
    }
    class InnerDestination
    {
        public int Value;
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap(typeof(Source), typeof(Destination<>)).ForMember("Inner", o => o.MapFrom("Inner"));
        c.CreateMap<InnerSource, InnerDestination>();
    });
    [Fact]
    public void Should_work() => Map<Destination<int>>(new Source { Inner = new() { Value = 42 } }).Inner.Value.ShouldBe(42);
}
public class ConstructorValidationGenerics : NonValidatingSpecBase
{
    record Source<T>(T Value);
    record Destination<T>(T OtherValue);
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap(typeof(Source<>), typeof(Destination<>)));
    [Fact]
    public void Should_work()
    {
        var error = new Action(AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>().Errors.Single();
        error.CanConstruct.ShouldBeFalse();
        error.UnmappedPropertyNames.Single().ShouldBe("OtherValue");
    }
}
public class SealGenerics : AutoMapperSpecBase
{
    public record SourceProperty<T>(T Value, SourceProperty<T> Recursive = null);
    public record DestProperty<T>(T Value, DestProperty<T> Recursive = null);
    public record User(SourceProperty<Guid> UserStoreId);
    public class UserPropertiesContainer
    {
        public UserProperties User { get; set; }
    }
    public class UserProperties
    {
        public DestProperty<Guid> UserStoreId { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(SourceProperty<>), typeof(DestProperty<>));
        cfg.CreateMap<User, UserPropertiesContainer>().ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
        cfg.CreateMap<User, UserProperties>();
    });
    [Fact]
    public void Should_work()
    {
        var guid = Guid.NewGuid();
        Map<UserProperties>(new User(new(guid))).UserStoreId.Value.ShouldBe(guid);
    }
}
public class OpenGenerics_With_Struct : AutoMapperSpecBase
{
    public struct Id<T>
    {
    }
    protected override MapperConfiguration CreateConfiguration() => new(mapper => mapper.CreateMap(typeof(Id<>), typeof(long)).ConvertUsing((_,__)=>(long)42));
    [Fact]
    public void Should_work() => Map<long>(new Id<string>()).ShouldBe(42);
}
public class OpenGenerics_With_Base_Generic : AutoMapperSpecBase
{
    public class Foo<T>
    {
        public T Value1 { get; set; }
    }
    public class BarBase<T>
    {
        public T Value2 { get; set; }
    }
    public class Bar<T> : BarBase<T>
    {
    }
    protected override MapperConfiguration CreateConfiguration() => new(mapper => mapper.CreateMap(typeof(Foo<>), typeof(Bar<>)).ForMember("Value2", to => to.MapFrom("Value1")));
    [Fact]
    public void Can_map_base_members() => Map<Bar<int>>(new Foo<int> { Value1 = 5 }).Value2.ShouldBe(5);
}
public class GenericMapsAsNonGeneric : AutoMapperSpecBase
{
    class Source
    {
        public int Value;
    }
    class Destination<T>
    {
        public T Value;
    }
    class NonGenericDestination : Destination<string>
    {
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(Source), typeof(Destination<>)).As(typeof(NonGenericDestination));
        cfg.CreateMap(typeof(Source), typeof(NonGenericDestination));
    });
    [Fact]
    public void Should_work() => Mapper.Map<Destination<string>>(new Source { Value = 42 }).Value.ShouldBe("42");
}
public class GenericMapsPriority : AutoMapperSpecBase
{
    class Source<T>
    {
        public T Value;
    }
    class Destination<T>
    {
        public T Value;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(Source<>), typeof(Destination<string>));
        cfg.CreateMap(typeof(Source<>), typeof(Destination<>)).ForAllMembers(o=>o.Ignore());
        cfg.CreateMap(typeof(Source<string>), typeof(Destination<>)).ForAllMembers(o => o.Ignore());
        cfg.CreateMap(typeof(Source<int>), typeof(Destination<>));
    });
    [Fact]
    public void Should_work()
    {
        Mapper.Map<Destination<int>>(new Source<int> { Value = 42 }).Value.ShouldBe(42);
        Mapper.Map<Destination<string>>(new Source<string> { Value = "42" }).Value.ShouldBe("42");
    }
}
public class GenericMapWithUntypedMap : AutoMapperSpecBase
{
    class Source<T>
    {
        public T Value;
    }
    class Destination<T>
    {
        public T Value;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>cfg.CreateMap(typeof(Source<>), typeof(Destination<>)));
    [Fact]
    public void Should_work() => new Action(() => Mapper.Map(new Source<int>(), null, typeof(Destination<>)))
        .ShouldThrow<ArgumentException>().Message.ShouldStartWith($"Type {typeof(Destination<>).FullName}[T] is a generic type definition");
}
public class GenericValueResolverTypeMismatch : AutoMapperSpecBase
{
    class Source<T>
    {
        public T Value;
    }
    class Destination
    {
        public string Value;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
        cfg.CreateMap(typeof(Source<>), typeof(Destination)).ForMember("Value", o => o.MapFrom(typeof(ValueResolver<>))));
    class ValueResolver<T> : IValueResolver<Source<T>, Destination, object>
    {
        public object Resolve(Source<T> source, Destination destination, object destMember, ResolutionContext context) => int.MaxValue;
    }
    [Fact]
    public void Should_map_ok() => Map<Destination>(new Source<object>()).Value.ShouldBe(int.MaxValue.ToString());
}
public class GenericValueResolver : AutoMapperSpecBase
{
    class Destination
    {
        public string MyKey;
        public string MyValue;
    }
    class Destination<TKey, TValue>
    {
        public TKey MyKey;
        public TValue MyValue;
    }

    class Source
    {
        public IEnumerable<int> MyValues;
    }

    class Source<T>
    {
        public IEnumerable<T> MyValues;
    }

    class Destination<T>
    {
        public IEnumerable<T> MyValues;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination))
            .ForMember("MyKey", o => o.MapFrom(typeof(KeyResolver<>)))
            .ForMember("MyValue", o => o.MapFrom(typeof(ValueResolver<,>)));
        cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination<,>))
            .ForMember("MyKey", o => o.MapFrom(typeof(KeyResolver<,,>)))
            .ForMember("MyValue", o => o.MapFrom(typeof(ValueResolver<,,,>)));
        cfg.CreateMap(typeof(Source), typeof(Destination<>))
            .ForMember("MyValues", o => o.MapFrom(typeof(ValuesResolver<>)));
        cfg.CreateMap(typeof(Source<>), typeof(Destination<>))
            .ForMember("MyValues", o => o.MapFrom(typeof(ValuesResolver<,>)));				
    });
    private class KeyResolver<TKey> : IValueResolver<KeyValuePair<TKey, int>, Destination, string>
    {
        public string Resolve(KeyValuePair<TKey, int> source, Destination destination, string destMember, ResolutionContext context) => source.Key.ToString();
    }
    private class ValueResolver<TKey, TValue> : IValueResolver<KeyValuePair<TKey, TValue>, Destination, string>
    {
        public string Resolve(KeyValuePair<TKey, TValue> source, Destination destination, string destMember, ResolutionContext context) => source.Value.ToString();
    }
    private class KeyResolver<TKeySource, TValueSource, TKeyDestination>
        : IValueResolver<KeyValuePair<TKeySource, TValueSource>, Destination<TKeyDestination, string>, string>
    {
        public string Resolve(KeyValuePair<TKeySource, TValueSource> source, Destination<TKeyDestination, string> destination, string destMember, ResolutionContext context)
            => source.Key.ToString();
    }
    private class ValueResolver<TKeySource, TValueSource, TKeyDestination, TValueDestination>
        : IValueResolver<KeyValuePair<TKeySource, TValueSource>, Destination<TKeyDestination, TValueDestination>, string>
    {
        public string Resolve(KeyValuePair<TKeySource, TValueSource> source, Destination<TKeyDestination, TValueDestination> destination, string destMember, ResolutionContext context)
            => source.Value.ToString();
    }
    private class ValuesResolver<TDestination>
        : IValueResolver<Source, Destination<TDestination>, IEnumerable<TDestination>>
    {
        public IEnumerable<TDestination> Resolve(Source source, Destination<TDestination> destination, IEnumerable<TDestination> destMember, ResolutionContext context)
        {
            foreach (var item in source.MyValues)
            {
                yield return (TDestination)((object)item);
            }
        }
    }
    private class ValuesResolver<TSource, TDestination>
        : IValueResolver<Source<TSource>, Destination<TDestination>, IEnumerable<TDestination>>
    {
        public IEnumerable<TDestination> Resolve(Source<TSource> source, Destination<TDestination> destination, IEnumerable<TDestination> destMember, ResolutionContext context)
        {
            foreach (var item in source.MyValues)
            {
                yield return (TDestination)((object)item);
            }
        }
    }
    [Fact]
    public void Should_map_non_generic_destination()
    {
        var destination = Map<Destination>(new KeyValuePair<int, int>(1,2));
        destination.MyKey.ShouldBe("1");
        destination.MyValue.ShouldBe("2");
    }
    [Fact]
    public void Should_map_generic_destination()
    {
        var destination = Map<Destination<string, string>>(new KeyValuePair<int, int>(1, 2));
        destination.MyKey.ShouldBe("1");
        destination.MyValue.ShouldBe("2");
        var destinationString = Map<Destination<string, string>>(new KeyValuePair<string, string>("1", "2"));
        destinationString.MyKey.ShouldBe("1");
        destinationString.MyValue.ShouldBe("2");
    }
    [Fact]
    public void Should_map_closed_to_ienumerable_generic_destination()
    {
        var source = new Source { MyValues = new int[] { 1, 2 } };
        var destination = Map<Destination<int>>(source);
        destination.MyValues.ShouldBe(source.MyValues);
    }
    [Fact]
    public void Should_map_ienumerable_generic_destination()
    {
        var source = new Source<int> { MyValues = new int[] { 1, 2 } };
        var destination = Map<Destination<int>>(source);
        destination.MyValues.ShouldBe(source.MyValues);
    }
}

public class GenericMemberValueResolver : AutoMapperSpecBase
{
    class Destination
    {
        public string MyKey;
        public string MyValue;
    }
    class Destination<TKey, TValue>
    {
        public TKey MyKey;
        public TValue MyValue;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination))
            .ForMember("MyKey", o => o.MapFrom(typeof(Resolver<>), "Key"))
            .ForMember("MyValue", o => o.MapFrom(typeof(Resolver<,>), "Value"));
        cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Destination<,>))
            .ForMember("MyKey", o => o.MapFrom(typeof(Resolver<,,>), "Key"))
            .ForMember("MyValue", o => o.MapFrom(typeof(Resolver<,,,>), "Value"));
    });
    private class Resolver<TKey> : IMemberValueResolver<KeyValuePair<TKey, int>, Destination, int, string>
    {
        public string Resolve(KeyValuePair<TKey, int> source, Destination destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
    }
    private class Resolver<TKey, TValue> : IMemberValueResolver<KeyValuePair<TKey, TValue>, Destination, int, string>
    {
        public string Resolve(KeyValuePair<TKey, TValue> source, Destination destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
    }
    private class Resolver<TKey, TValue, TDestinatonKey> : IMemberValueResolver<KeyValuePair<TKey, TValue>, Destination<TDestinatonKey, string>, int, string>
    {
        public string Resolve(KeyValuePair<TKey, TValue> source, Destination<TDestinatonKey, string> destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
    }
    private class Resolver<TKey, TValue, TDestinatonKey, TDestinatonValue> : IMemberValueResolver<KeyValuePair<TKey, TValue>, Destination<TDestinatonKey, TDestinatonValue>, int, string>
    {
        public string Resolve(KeyValuePair<TKey, TValue> source, Destination<TDestinatonKey, TDestinatonValue> destination, int sourceMember, string destMember, ResolutionContext context) => sourceMember.ToString();
    }
    [Fact]
    public void Should_map_non_generic_destination()
    {
        var destination = Map<Destination>(new KeyValuePair<int, int>(1, 2));
        destination.MyKey.ShouldBe("1");
        destination.MyValue.ShouldBe("2");
    }
    [Fact]
    public void Should_map_generic_destination()
    {
        var destination = Map<Destination<string, string>>(new KeyValuePair<int, int>(1, 2));
        destination.MyKey.ShouldBe("1");
        destination.MyValue.ShouldBe("2");
    }
}

public class RecursiveOpenGenerics : AutoMapperSpecBase
{
    public class SourceTree<T>
    {
        public SourceTree(T value, SourceTree<T>[] children)
        {
            Value = value;
            Children = children;
        }

        public T Value { get; }

        public SourceTree<T>[] Children { get; }
    }

    public class DestinationTree<T>
    {
        public DestinationTree(T value, DestinationTree<T>[] children)
        {
            Value = value;
            Children = children;
        }

        public T Value { get; }

        public DestinationTree<T>[] Children { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap(typeof(SourceTree<>), typeof(DestinationTree<>)));

    [Fact]
    public void Should_work()
    {
        var source = new SourceTree<string>("value", new SourceTree<string>[0]);
        Mapper.Map<DestinationTree<string>>(source).Value.ShouldBe("value");
    }
}

public class OpenGenericsValidation : NonValidatingSpecBase
{
    public class Source<T>
    {
        public T Value { get; set; }
    }

    public class Dest<T>
    {
        public int A { get; set; }
        public T Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

    [Fact]
    public void Should_report_unmapped_property()
    {
        new Action(Configuration.AssertConfigurationIsValid)
            .ShouldThrow<AutoMapperConfigurationException>()
            .Errors.Single().UnmappedPropertyNames.Single().ShouldBe("A");
    }
}

public class OpenGenericsProfileValidationNonGenericMembers : NonValidatingSpecBase
{
    public class Source<T>
    {
        public T[] Value { get; set; }
    }

    public class Dest<T>
    {
        public int A { get; set; }
        public T[] Value { get; set; }
    }

    class MyProfile : Profile
    {
        public MyProfile()
        {
            CreateMap(typeof(Source<>), typeof(Dest<>));
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.AddProfile<MyProfile>());

    [Fact]
    public void Should_report_unmapped_property() =>
        new Action(()=> AssertConfigurationIsValid<MyProfile>())
            .ShouldThrow<AutoMapperConfigurationException>()
            .Errors.Single().UnmappedPropertyNames.Single().ShouldBe("A");
}

public class OpenGenericsProfileValidation : AutoMapperSpecBase
{
    public class Source<T>
    {
        public T[] Value { get; set; }
    }

    public class Dest<T>
    {
        public T[] Value { get; set; }
    }

    class MyProfile : Profile
    {
        public MyProfile()
        {
            CreateMap(typeof(Source<>), typeof(Dest<>));
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.AddProfile<MyProfile>());
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class OpenGenerics
{
    public class Source<T>
    {
        public int A { get; set; }
        public T Value { get; set; }
    }

    public class Dest<T>
    {
        public int A { get; set; }
        public T Value { get; set; }
    }

    [Fact]
    public void Can_map_simple_generic_types()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

        var source = new Source<int>
        {
            Value = 5
        };

        var dest = config.CreateMapper().Map<Source<int>, Dest<int>>(source);

        dest.Value.ShouldBe(5);
    }

    [Fact]
    public void Can_map_non_generic_members()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

        var source = new Source<int>
        {
            A = 5
        };

        var dest = config.CreateMapper().Map<Source<int>, Dest<int>>(source);

        dest.A.ShouldBe(5);
    }

    [Fact]
    public void Can_map_recursive_generic_types()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

        var source = new Source<Source<int>>
        {
            Value = new Source<int>
            {
                Value = 5,
            }
        };

        var dest = config.CreateMapper().Map<Source<Source<int>>, Dest<Dest<double>>>(source);

        dest.Value.Value.ShouldBe(5);
    }
}

public class OpenGenerics_With_MemberConfiguration : AutoMapperSpecBase
{
    public class Foo<T>
    {
        public int A { get; set; }
        public int B { get; set; }
    }

    public class Bar<T>
    {
        public int C { get; set; }
        public int D { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(mapper => {
        mapper.CreateMap(typeof(Foo<>), typeof(Bar<>))
        .ForMember("C", to => to.MapFrom("A"))
        .ForMember("D", to => to.MapFrom("B"));
    });

    [Fact]
    public void Can_remap_explicit_members()
    {
        var source = new Foo<int>
        {
            A = 5,
            B = 10
        };

        var dest = Mapper.Map<Foo<int>, Bar<int>>(source);

        dest.C.ShouldBe(5);
        dest.D.ShouldBe(10);
    }
}

public class OpenGenerics_With_UntypedMapFrom : AutoMapperSpecBase
{
    public class Foo<T>
    {
        public T Value1 { get; set; }
    }

    public class Bar<T>
    {
        public T Value2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(mapper => {
        mapper.CreateMap(typeof(Foo<>), typeof(Bar<>)).ForMember("Value2", to => to.MapFrom("Value1"));
    });

    [Fact]
    public void Can_remap_explicit_members()
    {
        var dest = Mapper.Map<Bar<int>>(new Foo<int> { Value1 = 5 });
        dest.Value2.ShouldBe(5);
    }
}

public class OpenGenerics_With_UntypedMapFromStructs : AutoMapperSpecBase
{
    public class Foo<T> where T : struct
    {
        public T Value1 { get; set; }
    }

    public class Bar<T> where T : struct
    {
        public T Value2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(mapper => {
        mapper.CreateMap(typeof(Foo<>), typeof(Bar<>)).ForMember("Value2", to => to.MapFrom("Value1"));
    });

    [Fact]
    public void Can_remap_explicit_members()
    {
        var dest = Mapper.Map<Bar<int>>(new Foo<int> { Value1 = 5 });
        dest.Value2.ShouldBe(5);
    }
}