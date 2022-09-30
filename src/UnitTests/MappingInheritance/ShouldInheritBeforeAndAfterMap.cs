namespace AutoMapper.UnitTests.MappingInheritance;

public class ShouldInheritBeforeAndAfterMapOnlyOnce : AutoMapperSpecBase
{
    int afterMapCount;
    int beforeMapCount;

    public abstract class BaseBaseSource { }
    public class BaseSource : BaseBaseSource
    {
        public string Foo { get; set; }
    }
    public class Source : BaseSource { }

    public abstract class BaseBaseDest
    {
    }
    public class BaseDest : BaseBaseDest { }
    public class Dest : BaseDest { }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<BaseBaseSource, BaseBaseDest>().AfterMap((s, d) => afterMapCount++).BeforeMap((s, d)=>beforeMapCount++).Include<Source, Dest>().Include<BaseSource, BaseDest>();
        cfg.CreateMap<BaseSource, BaseDest>().Include<Source, Dest>();
        cfg.CreateMap<Source, Dest>();
    });

    protected override void Because_of()
    {
        Mapper.Map<Dest>(new Source());
    }

    [Fact]
    public void Should_call_AfterMap_just_once()
    {
        afterMapCount.ShouldBe(1);
        beforeMapCount.ShouldBe(1);
    }
}

public class ShouldInheritBeforeAndAfterMapOnlyOnceIncludeBase : AutoMapperSpecBase
{
    int afterMapCount;
    int beforeMapCount;

    public abstract class BaseBaseSource { }
    public class BaseSource : BaseBaseSource
    {
        public string Foo { get; set; }
    }
    public class Source : BaseSource { }

    public abstract class BaseBaseDest
    {
    }
    public class BaseDest : BaseBaseDest { }
    public class Dest : BaseDest { }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<BaseBaseSource, BaseBaseDest>().AfterMap((s, d) => afterMapCount++).BeforeMap((s, d) => beforeMapCount++);
        cfg.CreateMap<BaseSource, BaseDest>().IncludeBase<BaseBaseSource, BaseBaseDest>();
        cfg.CreateMap<Source, Dest>().IncludeBase<BaseSource, BaseDest>();
    });

    protected override void Because_of()
    {
        Mapper.Map<Dest>(new Source());
    }

    [Fact]
    public void Should_call_AfterMap_just_once()
    {
        afterMapCount.ShouldBe(1);
        beforeMapCount.ShouldBe(1);
    }
}

public class ShouldInheritBeforeAndAfterMap
{
    public class BaseClass
    {
        public string Prop { get; set; }
    } 
    public class Class : BaseClass {}

    public class BaseDto
    {
        public string DifferentProp { get; set; }            
    }
    public class Dto : BaseDto {}

    [Fact]
    public void should_inherit_base_beforemap()
    {
        // arrange
        var source = new Class{ Prop = "test" };
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg
                .CreateMap<BaseClass, BaseDto>()
                .BeforeMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            cfg.CreateMap<Class, Dto>();
        });
        var mappingEngine = configurationProvider.CreateMapper();

        // act
        var dest = mappingEngine.Map<Class, Dto>(source);

        // assert
        "test".ShouldBe(dest.DifferentProp);
    }

    [Fact]
    public void should_inherit_base_aftermap()
    {
        // arrange
        var source = new Class { Prop = "test" };
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg
                .CreateMap<BaseClass, BaseDto>()
                .AfterMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            cfg.CreateMap<Class, Dto>();
        });
        var mappingEngine = configurationProvider.CreateMapper();

        // act
        var dest = mappingEngine.Map<Class, Dto>(source);

        // assert
        "test".ShouldBe(dest.DifferentProp);
    }
}