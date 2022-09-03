namespace AutoMapper.UnitTests.Bug;
public class CorrectCtorIsPickedOnDestinationType : NonValidatingSpecBase
{
    public class SourceClass { }

    public class DestinationClass
    {
        public DestinationClass() { }

        // Since the name of the parameter is 'type', Automapper.TypeMap chooses SourceClass.GetType()
        // to fulfill the dependency, causing an InvalidCastException during Mapper.Map()
        public DestinationClass(Int32 type)
        {
            Type = type;
        }

        public Int32 Type { get; private set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<SourceClass, DestinationClass>());

    [Fact]
    public void Should_pick_a_ctor_which_best_matches()
    {
        var source = new SourceClass();

        Mapper.Map<DestinationClass>(source);
    }
}
public class MemberNamedTypeWrong : AutoMapperSpecBase
{
    public class SourceClass
    {
        public string Type { get; set; }
    }

    public class DestinationClass
    {
        public string Type { get; set; }
    }

    [Fact]
    public void Should_map_correctly()
    {
        var source = new SourceClass
        {
            Type = "Hello"
        };

        var result = Mapper.Map<DestinationClass>(source);
        result.Type.ShouldBe(source.Type);
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<SourceClass, DestinationClass>());
}