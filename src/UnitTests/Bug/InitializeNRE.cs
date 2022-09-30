namespace AutoMapper.UnitTests.Bug;

public class InitializeNRE2 : AutoMapperSpecBase
{
    public interface IRes : IValueResolver<Source, Destination, int>
    {
    }

    public class Res : IRes
    {
        public int Resolve(Source source, Destination destination, int destMember, ResolutionContext context)
        {
            return 1000;
        }
    }

    public class Source
    {
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.ConstructServicesUsing(t => new Res());
        cfg.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.MapFrom<IRes>());
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}


public class InitializeNRE : AutoMapperSpecBase
{
    public class TestEntity
    {
        public string SomeData { get; set; }

        public int SomeCount { get; set; }

        public ICollection<string> Tags { get; set; }
    }

    public class TestViewModel
    {
        public string SomeData { get; set; }

        public int SomeCount { get; set; }

        public string Tags { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<TestEntity, TestViewModel>();
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}