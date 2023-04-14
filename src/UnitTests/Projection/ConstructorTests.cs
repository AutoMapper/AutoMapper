namespace AutoMapper.UnitTests.Projection;

public class ConstructorLetClause : AutoMapperSpecBase
{
    class Source
    {
        public IList<SourceItem> Items { get; set; }
    }
    class SourceItem
    {
        public IList<SourceValue> Values { get; set; }
    }
    class SourceValue
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }
    class Destination
    {
        public Destination(DestinationItem item) => Item = item;
        public DestinationItem Item { get; }
    }
    class DestinationValue
    {
        public DestinationValue(int value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
        public int Value1 { get; }
        public int Value2 { get; }
    }
    class DestinationItem
    {
        public DestinationItem(DestinationValue destinationValue)
        {
            Value1 = destinationValue.Value1;
            Value2 = destinationValue.Value2;
        }
        public int Value1 { get; }
        public int Value2 { get; }
        public IList<DestinationValue> Values { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().ForCtorParam("item", o => o.MapFrom(s => s.Items.FirstOrDefault()));
        cfg.CreateProjection<SourceItem, DestinationItem>().ForCtorParam("destinationValue", o=>o.MapFrom(s=>s.Values.FirstOrDefault()));
        cfg.CreateProjection<SourceValue, DestinationValue>();
    });
    [Fact]
    public void Should_construct_correctly()
    {
        var query = new[] { new Source { Items = new[] { new SourceItem { Values = new[] { new SourceValue { Value1 = 1, Value2 = 2 } } } } } }.AsQueryable().ProjectTo<Destination>(Configuration);
        var first = query.First();
        first.Item.Value1.ShouldBe(1);
        first.Item.Value2.ShouldBe(2);
        var firstValue = first.Item.Values.Single();
        firstValue.Value1.ShouldBe(1);
        firstValue.Value2.ShouldBe(2);
    }
}
public class ConstructorToString : AutoMapperSpecBase
{
    class Source
    {
        public int Value { get; set; }
    }
    class Destination
    {
        public Destination(string value) => Value = value;
        public string Value { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Source, Destination>());
    [Fact]
    public void Should_construct_correctly() => new[] { new Source { Value = 5 } }.AsQueryable().ProjectTo<Destination>(Configuration).First().Value.ShouldBe("5");
}
public class ConstructorMapFrom : AutoMapperSpecBase
{
    class Source
    {
        public int Value { get; set; }
    }
    record Destination(bool Value)
    {
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => 
        cfg.CreateProjection<Source, Destination>().ForCtorParam(nameof(Destination.Value), o=>o.MapFrom(s=>s.Value==5)));
    [Fact]
    public void Should_construct_correctly() => new[] { new Source { Value = 5 } }.AsQueryable().ProjectTo<Destination>(Configuration).First().Value.ShouldBeTrue();
}
public class ConstructorIncludeMembers : AutoMapperSpecBase
{
    class SourceWrapper
    {
        public Source Source { get; set; }
    }
    class Source
    {
        public int Value { get; set; }
    }
    class Destination
    {
        public Destination(string value) => Value = value;
        public string Value { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<SourceWrapper, Destination>().IncludeMembers(s => s.Source);
        cfg.CreateProjection<Source, Destination>();
    });
    [Fact]
    public void Should_construct_correctly() => new[] { new SourceWrapper { Source = new Source { Value = 5 } } }.AsQueryable().ProjectTo<Destination>(Configuration).First().Value.ShouldBe("5");
}
public class ConstructorsWithCollections : AutoMapperSpecBase
{
    class Addresses
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public ICollection<Users> Users { get; set; }
    }
    class Users
    {
        public int Id { get; set; }
        public Addresses FkAddress { get; set; }
    }
    class AddressDto
    {
        public int Id { get; }
        public string Address { get; }
        public AddressDto(int id, string address)
        {
            Id = id;
            Address = address;
        }
    }
    class UserDto
    {
        public int Id { get; set; }
        public AddressDto AddressDto { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateProjection<Users, UserDto>().ForMember(d => d.AddressDto, e => e.MapFrom(s => s.FkAddress));
        cfg.CreateProjection<Addresses, AddressDto>().ConstructUsing(a => new AddressDto(a.Id, a.Address));
    });
    [Fact]
    public void Should_work() => ProjectTo<UserDto>(new[] { new Users { FkAddress = new Addresses { Address = "address" }  } }.AsQueryable()).First().AddressDto.Address.ShouldBe("address");
}
public class ConstructorTests : AutoMapperSpecBase
{
    private Dest[] _dest;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest()
        {
            
        }
        public Dest(int other)
        {
            Other = other;
        }

        public int Value { get; set; }
        [IgnoreMap]
        public int Other { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AddIgnoreMapAttribute();
        cfg.CreateProjection<Source, Dest>()
            .ConstructUsing(src => new Dest(src.Value + 10));
    });

    protected override void Because_of()
    {
        var values = new[]
        {
            new Source()
            {
                Value = 5
            }
        }.AsQueryable();

        _dest = values.ProjectTo<Dest>(Configuration).ToArray();
    }

    [Fact]
    public void Should_construct_correctly()
    {
        _dest[0].Other.ShouldBe(15);
    }
}
public class NestedConstructors : AutoMapperSpecBase
{
    public class A
    {
        public int Id { get; set; }
        public B B { get; set; }
    }
    public class B
    {
        public int Id { get; set; }
    }
    public class DtoA
    {
        public DtoB B { get; }
        public DtoA(DtoB b) => B = b;
    }
    public class DtoB
    {
        public int Id { get; }
        public DtoB(int id) => Id = id;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<A, DtoA>();
        cfg.CreateProjection<B, DtoB>();
    });
    [Fact]
    public void Should_project_ok() =>
        ProjectTo<DtoA>(new[] { new A { B = new B { Id = 3 } } }.AsQueryable()).FirstOrDefault().B.Id.ShouldBe(3);
}

public class ConstructorLetClauseWithIheritance : AutoMapperSpecBase
{
    class Source
    {
        public IList<SourceItem> Items { get; set; }
    }
    class SourceA : Source
    {
        public string A { get; set; }
    }
    class SourceB : Source
    {
        public string B { get; set; }
    }

    class SourceItem
    {
        public IList<SourceValue> Values { get; set; }
    }
    class SourceValue
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }
    class Destination
    {
        public Destination(DestinationItem item) => Item = item;
        public DestinationItem Item { get; }
    }
    class DestinationA : Destination
    {
        public DestinationA(DestinationItem item, string a) : base(item) => A = a;
        public string A { get; }
    }
    class DestinationB : Destination
    {
        public DestinationB(DestinationItem item, string b) : base(item) => B = b;
        public string B { get; }
    }

    class DestinationValue
    {
        public DestinationValue(int value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
        public int Value1 { get; }
        public int Value2 { get; }
    }
    class DestinationItem
    {
        public DestinationItem(DestinationValue destinationValue)
        {
            Value1 = destinationValue.Value1;
            Value2 = destinationValue.Value2;
        }
        public int Value1 { get; }
        public int Value2 { get; }
        public IList<DestinationValue> Values { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForCtorParam("item", o => o.MapFrom(s => s.Items.FirstOrDefault()))
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>()
            .ForCtorParam("item", o => o.MapFrom(s => s.Items.FirstOrDefault()))
            .ForCtorParam("a", o => o.MapFrom(s => s.A));
        cfg.CreateMap<SourceB, DestinationB>()
            .ForCtorParam("item", o => o.MapFrom(s => s.Items.FirstOrDefault()))
            .ForCtorParam("b", o => o.MapFrom(s => s.B));

        cfg.CreateMap<SourceItem, DestinationItem>().ForCtorParam("destinationValue", o => o.MapFrom(s => s.Values.FirstOrDefault()));
        cfg.CreateMap<SourceValue, DestinationValue>();
    });
    [Fact]
    public void Should_construct_correctly()
    {
        var query = new[] {
            new SourceA
            {
                A = "a",
                Items = new[]
                {
                    new SourceItem { Values = new[] { new SourceValue { Value1 = 1, Value2 = 2 } } }
                }
            },
            new SourceB
            {
                B = "b",
                Items = new[]
                {
                    new SourceItem { Values = new[] { new SourceValue { Value1 = 1, Value2 = 2 } } }
                }
            },
            new Source
            {
                Items = new[]
                {
                    new SourceItem { Values = new[] { new SourceValue { Value1 = 1, Value2 = 2 } } }
                }
            }
        }.AsQueryable().ProjectTo<Destination>(Configuration);

        var list = query.ToList();
        var first = list.First();
        first.Item.Value1.ShouldBe(1);
        first.Item.Value2.ShouldBe(2);
        var firstValue = first.Item.Values.Single();
        firstValue.Value1.ShouldBe(1);
        firstValue.Value2.ShouldBe(2);

        list.OfType<DestinationA>().Any(a => a.A == "a").ShouldBeTrue();
        list.OfType<DestinationB>().Any(a => a.B == "b").ShouldBeTrue();
    }
}
public class ConstructorToStringWithIheritance : AutoMapperSpecBase
{
    class Source
    {
        public int Value { get; set; }
    }
    class SourceA : Source
    {
        public string A { get; set; }
    }
    class SourceB : Source
    {
        public string B { get; set; }
    }
    class Destination
    {
        public Destination(string value) => Value = value;
        public string Value { get; }
    }
    class DestinationA : Destination
    {
        public DestinationA(string value, string a) : base(value) => A = a;
        public string A { get; }
    }
    class DestinationB : Destination
    {
        public DestinationB(string value, string b) : base(value) => B = b;
        public string B { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .Include<SourceA, DestinationA>();
        cfg.CreateMap<SourceA, DestinationA>();
        cfg.CreateMap<SourceB, DestinationB>();
    });
    [Fact]
    public void Should_construct_correctly()
    {
        var list = new[]
        {
            new Source { Value = 5 },
            new SourceA { Value = 5, A = "a" },
            new SourceB { Value = 5, B = "b" }
        }.AsQueryable().ProjectTo<Destination>(Configuration);

        list.ShouldAllBe(p => p.Value == "5");
        list.OfType<DestinationA>().Any(p => p.A == "a");
        list.OfType<DestinationB>().Any(p => p.B == "b");
    }
}
public class ConstructorMapFromWithIheritance : AutoMapperSpecBase
{
    class Source
    {
        public int Value { get; set; }
    }
    class SourceA : Source
    {
        public string A { get; set; }
    }
    class SourceB : Source
    {
        public string B { get; set; }
    }
    record Destination(bool Value)
    {
    }
    record DestinationA(bool Value, bool HasA) : Destination(Value)
    {
    }
    record DestinationB(bool Value, bool HasB) : Destination(Value)
    {
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForCtorParam(nameof(Destination.Value), o => o.MapFrom(s => s.Value == 5))
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>()
            .ForCtorParam(nameof(Destination.Value), o => o.MapFrom(s => s.Value == 5))
            .ForCtorParam(nameof(DestinationA.HasA), o => o.MapFrom(s => s.A == "a"));
        cfg.CreateMap<SourceB, DestinationB>()
            .ForCtorParam(nameof(Destination.Value), o => o.MapFrom(s => s.Value == 5))
            .ForCtorParam(nameof(DestinationB.HasB), o => o.MapFrom(s => s.B == "b"));
    });
    [Fact]
    public void Should_construct_correctly()
    {
        var list = new[]
        {
            new Source { Value = 5 },
            new SourceA { Value = 5, A = "a" },
            new SourceB { Value = 5, B = "b" }
        }.AsQueryable().ProjectTo<Destination>(Configuration);

        list.All(p => p.Value).ShouldBeTrue();
        list.OfType<DestinationA>().Any(p => p.HasA).ShouldBeTrue();
        list.OfType<DestinationB>().Any(p => p.HasB).ShouldBeTrue();
    }
}
public class ConstructorIncludeMembersWithIheritance : AutoMapperSpecBase
{
    class SourceWrapper
    {
        public Source Source { get; set; }
    }
    class SourceWrapperA : SourceWrapper
    {
        public SourceA SourceA { get; set; }
    }
    class SourceWrapperB : SourceWrapper
    {
        public SourceB SourceB { get; set; }
    }
    class Source
    {
        public int Value { get; set; }
    }
    class SourceA
    {
        public string A { get; set; }
    }
    class SourceB
    {
        public string B { get; set; }
    }
    class Destination
    {
        public Destination(string value) => Value = value;
        public string Value { get; }
    }

    class DestinationA : Destination
    {
        public DestinationA(string value, string a) : base(value) => A = a;
        public string A { get; }
    }
    class DestinationB : Destination
    {
        public DestinationB(string value, string b) : base(value) => B = b;
        public string B { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<SourceWrapper, Destination>()
            .IncludeMembers(s => s.Source)
            .Include<SourceWrapperA, DestinationA>()
            .Include<SourceWrapperB, DestinationB>();
        cfg.CreateMap<SourceWrapperA, DestinationA>()
            .IncludeMembers(s => s.Source, s => s.SourceA);
        cfg.CreateMap<SourceWrapperB, DestinationB>()
            .IncludeMembers(s => s.Source, s => s.SourceB);
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<Source, DestinationA>(MemberList.None);
        cfg.CreateMap<Source, DestinationB>(MemberList.None);
        cfg.CreateMap<SourceA, DestinationA>(MemberList.None);
        cfg.CreateMap<SourceB, DestinationB>(MemberList.None);
    });
    [Fact]
    public void Should_construct_correctly()
    {
        var list = new[]
        {
            new SourceWrapper { Source = new Source { Value = 5 } },
            new SourceWrapperA { Source = new Source { Value = 5 }, SourceA = new SourceA() { A = "a" } },
            new SourceWrapperB { Source = new Source { Value = 5 }, SourceB = new SourceB() { B = "b" } }
        }.AsQueryable().ProjectTo<Destination>(Configuration);

        list.All(p => p.Value == "5").ShouldBeTrue();
        list.OfType<DestinationA>().Any(p => p.A == "a").ShouldBeTrue();
        list.OfType<DestinationB>().Any(p => p.B == "b").ShouldBeTrue();
    }
}
public class ConstructorsWithCollectionsWithIheritance : AutoMapperSpecBase
{
    class Addresses
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public ICollection<Users> Users { get; set; }
    }
    class Users
    {
        public int Id { get; set; }
        public Addresses FkAddress { get; set; }
    }
    class UsersA : Users
    {
        public string A { get; set; }
    }
    class UsersB : Users
    {
        public string B { get; set; }
    }
    class AddressDto
    {
        public int Id { get; }
        public string Address { get; }
        public AddressDto(int id, string address)
        {
            Id = id;
            Address = address;
        }
    }
    class UserDto
    {
        public int Id { get; set; }
        public AddressDto AddressDto { get; set; }
    }
    class UserADto : UserDto
    {
        public string A { get; set; }
    }
    class UserBDto : UserDto
    {
        public string B { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Users, UserDto>()
            .ForMember(d => d.AddressDto, e => e.MapFrom(s => s.FkAddress))
            .Include<UsersA, UserADto>()
            .Include<UsersB, UserBDto>();

        cfg.CreateMap<UsersA, UserADto>();
        cfg.CreateMap<UsersB, UserBDto>();

        cfg.CreateMap<Addresses, AddressDto>().ConstructUsing(a => new AddressDto(a.Id, a.Address));
    });
    [Fact]
    public void Should_work()
    {
        var list = ProjectTo<UserDto>(new[]
        {
            new Users { FkAddress = new Addresses { Address = "address" } },
            new UsersA { A = "a", FkAddress = new Addresses { Address = "address" } },
            new UsersB { B = "b", FkAddress = new Addresses { Address = "address" } }
        }.AsQueryable()).ToList();

        list.All(p => p.AddressDto.Address == "address").ShouldBeTrue();
        list.OfType<UserADto>().Any(p => p.A == "a").ShouldBeTrue();
        list.OfType<UserBDto>().Any(p => p.B == "b").ShouldBeTrue();
    }
}
public class ConstructorTestsWithIheritance : AutoMapperSpecBase
{
    private Dest[] _dest;

    public class Source
    {
        public int Value { get; set; }
    }
    public class SourceA : Source
    {
        public string A { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }

    public class Dest
    {
        public Dest()
        {

        }
        public Dest(int other)
        {
            Other = other;
        }

        public int Value { get; set; }
        [IgnoreMap]
        public int Other { get; set; }
    }
    public class DestA : Dest
    {
        public DestA() : base()
        {

        }
        public DestA(int other, string otherA) : base(other)
        {
            OtherA = otherA;
        }
        [IgnoreMap]
        public string OtherA { get; set; }
    }
    public class DestB : Dest
    {
        public DestB() : base()
        {

        }
        public DestB(int other, string otherB) : base(other)
        {
            OtherB = otherB;
        }
        [IgnoreMap]
        public string OtherB { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AddIgnoreMapAttribute();
        cfg.CreateMap<Source, Dest>()
            .ConstructUsing(src => new Dest(src.Value + 10))
            .Include<SourceA, DestA>()
            .Include<SourceB, DestB>();

        cfg.CreateMap<SourceA, DestA>()
            .ConstructUsing(src => new DestA(src.Value + 10, src.A + "a"));

        cfg.CreateMap<SourceB, DestB>()
            .ConstructUsing(src => new DestB(src.Value + 10, src.B + "b"));
    });

    protected override void Because_of()
    {
        var values = new[]
        {
            new Source()
            {
                Value = 5
            },
            new SourceA()
            {
                Value = 5,
                A = "a"
            },
            new SourceB()
            {
                Value = 5,
                B = "b"
            }
        }.AsQueryable();

        _dest = values.ProjectTo<Dest>(Configuration).ToArray();
    }

    [Fact]
    public void Should_construct_correctly()
    {
        _dest.All(p => p.Other == 15).ShouldBeTrue();
        _dest.OfType<DestA>().Any(p => p.OtherA == "aa").ShouldBeTrue();
        _dest.OfType<DestB>().Any(p => p.OtherB == "bb").ShouldBeTrue();
    }
}
public class NestedConstructorsWithIheritance : AutoMapperSpecBase
{
    public class A
    {
        public int Id { get; set; }
        public B B { get; set; }
    }
    public class B
    {
        public int Id { get; set; }
    }
    public class DtoA
    {
        public DtoB B { get; }
        public DtoA(DtoB b) => B = b;
    }
    public class DtoB
    {
        public int Id { get; }
        public DtoB(int id) => Id = id;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<A, DtoA>();
        cfg.CreateProjection<B, DtoB>();
    });
    [Fact]
    public void Should_project_ok() =>
        ProjectTo<DtoA>(new[] { new A { B = new B { Id = 3 } } }.AsQueryable()).FirstOrDefault().B.Id.ShouldBe(3);
}