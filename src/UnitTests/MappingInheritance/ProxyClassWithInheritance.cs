namespace AutoMapper.UnitTests.MappingInheritance
{
    using Should;
    using Xunit;

    public class ProxyClassWithInheritance : SpecBase
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

            dto.ShouldBeType<DuckDto>();
        }
    }
}