namespace AutoMapper.UnitTests;

public class OpenGenericsWithAs : AutoMapperSpecBase
{
    public class Source
    {
        public object Value { get; set; }
    }

    public interface ITarget<T>
    {
        T Value { get; }
    }

    public class Target<T> : ITarget<T>
    {
        public T Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap(typeof(Source), typeof(Target<>));
        cfg.CreateMap(typeof(Source), typeof(ITarget<>)).As(typeof(Target<>));
    });

    [Fact]
    public void Should_use_the_redirected_map()
    {
        var source = new Source { Value = "value" };
        Mapper.Map<ITarget<string>>(source).Value.ShouldBe(source.Value);
    }
}

public class OpenGenericsWithInclude : AutoMapperSpecBase
{
    public class Person
    {
        public string Name { get; set; }
        public List<BarBase> BarList { get; set; } = new List<BarBase>();
    }

    public class PersonModel
    {
        public string Name { get; set; }
        public List<BarModelBase> BarList { get; set; }
    }

    abstract public class BarBase
    {
        public int Id { get; set; }
    }

    public class Bar<T> : BarBase
    {
        public T Value { get; set; }
    }

    abstract public class BarModelBase
    {
        public int Id { get; set; }
        public string Ignored { get; set; }
        public string MappedFrom { get; set; }
    }

    public class BarModel<T> : BarModelBase
    {
        public T Value { get; set; }
        public string DerivedMember { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<BarBase, BarModelBase>()
            .ForMember(d=>d.Ignored, o=>o.Ignore())
            .ForMember(d=>d.MappedFrom, o=>o.MapFrom(_=>"mappedFrom"))
            .Include(typeof(Bar<>), typeof(BarModel<>));
        cfg.CreateMap<Person, PersonModel>();
        cfg.CreateMap(typeof(Bar<>), typeof(BarModel<>)).ForMember("DerivedMember", o=>o.MapFrom("Id"));
    });

    [Fact]
    public void Should_work()
    {
        var person = new Person { Name = "Jack", BarList = { new Bar<string>{ Id = 1, Value = "One" }, new Bar<string>{ Id = 2, Value = "Two" } } };

        var personMapped = Mapper.Map<PersonModel>(person);

        var barModel = (BarModel<string>)personMapped.BarList[0];
        barModel.Value.ShouldBe("One");
        barModel.DerivedMember.ShouldBe("1");
        barModel.MappedFrom.ShouldBe("mappedFrom");
        barModel = (BarModel<string>)personMapped.BarList[1];
        barModel.Value.ShouldBe("Two");
        barModel.DerivedMember.ShouldBe("2");
        barModel.MappedFrom.ShouldBe("mappedFrom");
    }
}

public class OpenGenericsWithIncludeBase : AutoMapperSpecBase
{
    public class Person
    {
        public string Name { get; set; }
        public List<BarBase> BarList { get; set; } = new List<BarBase>();
    }

    public class PersonModel
    {
        public string Name { get; set; }
        public List<BarModelBase> BarList { get; set; }
    }

    abstract public class BarBase
    {
        public int Id { get; set; }
    }

    public class Bar<T> : BarBase
    {
        public T Value { get; set; }
    }

    abstract public class BarModelBase
    {
        public int Id { get; set; }
        public string Ignored { get; set; }
        public string MappedFrom { get; set; }
    }

    public class BarModel<T> : BarModelBase
    {
        public T Value { get; set; }
        public string DerivedMember { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(BarBase), typeof(BarModelBase))
            .ForMember("Ignored", o => o.Ignore())
            .ForMember("MappedFrom", o => o.MapFrom(_=>"mappedFrom"));
        cfg.CreateMap<Person, PersonModel>();
        cfg.CreateMap(typeof(Bar<>), typeof(BarModel<>))
            .ForMember("DerivedMember", o => o.MapFrom("Id"))
            .IncludeBase(typeof(BarBase), typeof(BarModelBase));
    });

    [Fact]
    public void Should_work()
    {
        var person = new Person { Name = "Jack", BarList = { new Bar<string> { Id = 1, Value = "One" }, new Bar<string> { Id = 2, Value = "Two" } } };

        var personMapped = Mapper.Map<PersonModel>(person);

        var barModel = (BarModel<string>)personMapped.BarList[0];
        barModel.Value.ShouldBe("One");
        barModel.DerivedMember.ShouldBe("1");
        barModel.MappedFrom.ShouldBe("mappedFrom");
        barModel = (BarModel<string>)personMapped.BarList[1];
        barModel.Value.ShouldBe("Two");
        barModel.DerivedMember.ShouldBe("2");
        barModel.MappedFrom.ShouldBe("mappedFrom");
    }
}

public class OpenGenericsAndNonGenericsWithIncludeBase : AutoMapperSpecBase
{
    public abstract class Entity
    {
        public string BaseMember { get; set; }
    }

    public abstract class Model
    {
        public string BaseMember { get; set; }
    }

    public abstract class Entity<TId> : Entity
    {
        public TId Id { get; set; }
    }

    public abstract class Model<TId> : Model
    {
        public TId Id { get; set; }
    }

    public class SubEntity : Entity<int>
    {
        public string SubMember { get; set; }
    }

    public class SubModel : Model<int>
    {
        public string SubMember { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Entity, Model>();

        cfg.CreateMap(typeof(Entity<>), typeof(Model<>))
            .IncludeBase(typeof(Entity), typeof(Model));


        cfg.CreateMap<SubEntity, SubModel>()
            .IncludeBase<Entity<int>, Model<int>>()
            .IncludeBase<Entity, Model>();
    });

    [Fact]
    public void Should_work()
    {
        var entity = new SubEntity { BaseMember = "foo", Id = 695, SubMember = "bar" };

        var model = this.Mapper.Map<SubModel>(entity);

        model.BaseMember.ShouldBe("foo");
        model.Id.ShouldBe(695);
        model.SubMember.ShouldBe("bar");
    }
}
public class IncludeBaseOpenGenerics : AutoMapperSpecBase
{
    public abstract class OrderModel<T>
    {
        public string Number { get; set; }
    }
    public class InternetOrderModel : OrderModel<int>
    {
    }
    public abstract class Order<T>
    {
        public string OrderNumber { get; set; }
    }
    public class InternetOrder : Order<int>
    {
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap(typeof(OrderModel<>), typeof(Order<>))
            .ForMember("OrderNumber", o => o.MapFrom("Number"));
        c.CreateMap<InternetOrderModel, InternetOrder>()
            .IncludeBase(typeof(OrderModel<>), typeof(Order<>));
    });
    [Fact]
    public void Shoud_work() => Mapper.Map<InternetOrder>(new InternetOrderModel { Number = "42" }).OrderNumber.ShouldBe("42");
}