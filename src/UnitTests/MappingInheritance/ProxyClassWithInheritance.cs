namespace AutoMapper.UnitTests.MappingInheritance
{
    using Should;
    using Xunit;

    public class ProxyClassWithInheritance : AutoMapperSpecBase
    {
        public class Duck : Animal { }
        public class DuckDto : AnimalDto { } 
        public abstract class Animal { }
        public abstract class AnimalDto { }
        public class DuckProxyClassFoo : Duck { }

        [Fact]
        public void Should_map_correctly()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Animal, AnimalDto>()
                    .Include<Duck, DuckDto>();

                cfg.CreateMap<Duck, DuckDto>().ReverseMap();
            });

            var aDuck = new DuckProxyClassFoo();

            var dto = Mapper.Map<Animal, AnimalDto>(aDuck);

            dto.ShouldBeType<DuckDto>();
        }
    }
}