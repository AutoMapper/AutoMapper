namespace AutoMapper.UnitTests.MappingInheritance;
public class ReverseMapWithInclude : NonValidatingSpecBase
{
    public class Duck : Animal { }
    public class DuckDto : AnimalDto { } 
    public abstract class Animal { }
    public abstract class AnimalDto { }
    public class DuckProxyClassFoo : Duck { }

    [Fact]
    public void Should_map_correctly()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Animal, AnimalDto>()
                .Include<Duck, DuckDto>();

            cfg.CreateMap<Duck, DuckDto>().ReverseMap();
        });

        var aDuck = new DuckProxyClassFoo();

        var mapper = config.CreateMapper();
        var dto = mapper.Map<Animal, AnimalDto>(aDuck);

        dto.ShouldBeOfType<DuckDto>();
    }
}

public class ReverseMapWithIncludeBase : AutoMapperSpecBase
{
    ConcreteSource _destination;

    public class Destination
    {
        public string Name { get; set; }
    }

    public class ConcreteDestination : Destination { }

    public class Source
    {
        public string Title { get; set; }
    }

    public class ConcreteSource : Source { }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Name, conf => conf.MapFrom(source => source.Title))
            .ReverseMap()
            .ForMember(dest => dest.Title, conf => conf.MapFrom(source => source.Name));
        cfg.CreateMap<ConcreteSource, ConcreteDestination>()
            .IncludeBase<Source, Destination>()
            .ReverseMap();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<ConcreteSource>(new ConcreteDestination { Name = "Name" });
    }

    [Fact]
    public void Should_work_together()
    {
        _destination.Title.ShouldBe("Name");
    }
}