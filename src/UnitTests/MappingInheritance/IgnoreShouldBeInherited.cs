namespace AutoMapper.UnitTests.Bug;
public class IgnoreShouldBeInheritedRegardlessOfMapOrder : AutoMapperSpecBase
{
    public class BaseDomain
    {
    }

    public class SpecificDomain : BaseDomain
    {
        public string SpecificProperty { get; set; }
    }

    public class Dto
    {
        public string SpecificProperty { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<SpecificDomain, Dto>();
        cfg.CreateMap<BaseDomain, Dto>()
            .ForMember(d => d.SpecificProperty, m => m.Ignore())
            .Include<SpecificDomain, Dto>();
    });

    [Fact]
    public void Should_map_ok()
    {
        var dto = Mapper.Map<Dto>(new SpecificDomain { SpecificProperty = "Test" });
        dto.SpecificProperty.ShouldBeNull();
    }
}

public class IgnoreShouldBeInherited : AutoMapperSpecBase
{
    public class BaseDomain
    {            
    }

    public class SpecificDomain : BaseDomain
    {
        public string SpecificProperty { get; set; }            
    }

    public class Dto
    {
        public string SpecificProperty { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<BaseDomain, Dto>()
            .ForMember(d => d.SpecificProperty, m => m.Ignore())
            .Include<SpecificDomain, Dto>();
        cfg.CreateMap<SpecificDomain, Dto>();
    });

    [Fact]
    public void Should_map_ok()
    {
        var dto = Mapper.Map<Dto>(new SpecificDomain { SpecificProperty = "Test" });
        dto.SpecificProperty.ShouldBeNull();
    }
}

public class IgnoreShouldBeInheritedWithOpenGenerics : AutoMapperSpecBase
{
    public abstract class BaseUserDto<TIdType>
    {
        public TIdType Id { get; set; }
        public string Name { get; set; }
    }

    public class ConcreteUserDto : BaseUserDto<string>
    {
    }

    public abstract class BaseUserEntity<TIdType>
    {
        public TIdType Id { get; set; }
        public string Name { get; set; }
    }

    public class ConcreteUserEntity : BaseUserEntity<string>
    {
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(BaseUserDto<>), typeof(BaseUserEntity<>)).ForMember("Id", opt => opt.Ignore());
        cfg.CreateMap(typeof(ConcreteUserDto), typeof(ConcreteUserEntity)).IncludeBase(typeof(BaseUserDto<string>), typeof(BaseUserEntity<string>));
    });

    [Fact]
    public void Should_map_ok()
    {
        var user = new ConcreteUserDto
        {
            Id = "my-id",
            Name = "my-User"
        };
        var userEntity = Mapper.Map<ConcreteUserEntity>(user);
        userEntity.Id.ShouldBeNull();
        userEntity.Name.ShouldBe("my-User");
    }
}
public class IgnoreOverrideShouldBeInherited : AutoMapperSpecBase
{
    class Foo
    {
        public string FooText { get; set; }
        public string Text { get; set; }
    }
    class Bar { public string FooText { get; set; } }
    class Boo : Bar { }
    protected override MapperConfiguration CreateConfiguration() => new(c=>
    {
        c.CreateMap<object, Foo>().ForMember(d => d.FooText, o => o.Ignore()).ForMember(d=>d.Text, o=>o.MapFrom(s=>"")).Include<Bar, Foo>();
        c.CreateMap<Bar, Foo>().ForMember(d => d.FooText, o => o.MapFrom(s => s.FooText)).Include<Boo, Foo>();
        c.CreateMap<Boo, Foo>();
    });
    [Fact]
    public void Should_map()
    {
        var dest = Map<Foo>(new Boo { FooText = "hi" });
        dest.FooText.ShouldBe("hi");
        dest.Text.ShouldBe("");
    }
}
public class IgnoreOverrideShouldBeOverriden : AutoMapperSpecBase
{
    class Foo
    {
        public string FooText { get; set; }
        public string Text { get; set; }
    }
    class Bar { public string FooText { get; set; } }
    class Boo : Bar { }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<object, Foo>().ForMember(d => d.FooText, o => o.Ignore()).ForMember(d => d.Text, o => o.MapFrom(s=>"")).Include<Bar, Foo>();
        c.CreateMap<Bar, Foo>().ForMember(d => d.FooText, o => o.MapFrom(s => s.FooText)).Include<Boo, Foo>();
        c.CreateMap<Boo, Foo>().ForMember(d => d.FooText, o => o.Ignore());
    });
    [Fact]
    public void Should_not_map()
    {
        var dest = Map<Foo>(new Boo { FooText = "hi" });
        dest.FooText.ShouldBeNull();
        dest.Text.ShouldBe("");
    }
}
public class IgnoreOverrideShouldBeInheritedIncludeBase : AutoMapperSpecBase
{
    class Foo
    {
        public string FooText { get; set; }
        public string Text { get; set; }
    }
    class Bar { public string FooText { get; set; } }
    class Boo : Bar { }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<object, Foo>().ForMember(d => d.FooText, o => o.Ignore()).ForMember(d => d.Text, o => o.MapFrom(s=>""));
        c.CreateMap<Bar, Foo>().ForMember(d => d.FooText, o => o.MapFrom(s => s.FooText)).IncludeBase<object, Foo>();
        c.CreateMap<Boo, Foo>().IncludeBase<Bar, Foo>();
    });
    [Fact]
    public void Should_map()
    {
        var dest = Map<Foo>(new Boo { FooText = "hi" });
        dest.FooText.ShouldBe("hi");
        dest.Text.ShouldBe("");
    }
}
public class IgnoreOverrideShouldBeOverridenIncludeBase : AutoMapperSpecBase
{
    class Foo
    {
        public string FooText { get; set; }
        public string Text { get; set; }
    }
    class Bar { public string FooText { get; set; } }
    class Boo : Bar { }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<object, Foo>().ForMember(d => d.FooText, o => o.Ignore()).ForMember(d => d.Text, o => o.MapFrom(s=>""));
        c.CreateMap<Bar, Foo>().ForMember(d => d.FooText, o => o.MapFrom(s => s.FooText)).IncludeBase<object, Foo>();
        c.CreateMap<Boo, Foo>().ForMember(d => d.FooText, o => o.Ignore()).IncludeBase<Bar, Foo>();
    });
    [Fact]
    public void Should_not_map()
    {
        var dest = Map<Foo>(new Boo { FooText = "hi" });
        dest.FooText.ShouldBeNull();
        dest.Text.ShouldBe("");
    }
}