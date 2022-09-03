namespace AutoMapper.UnitTests.Bug;

public class ObjectEnumToObjectEnum : AutoMapperSpecBase
{
    Target _target;

    public enum SourceEnumValue
    {
        Donkey,
        Mule
    }

    public enum TargetEnumValue
    {
        Donkey,
        Mule
    }

    public class Source
    {
        public object Value { get; set; }
    }

    public class Target
    {
        public object Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        var parentMapping = cfg.CreateMap<Source, Target>();
        parentMapping.ForMember(dest => dest.Value, opt => opt.MapFrom(s => (TargetEnumValue) s.Value));
    });

    protected override void Because_of()
    {
        _target = Mapper.Map<Target>(new Source { Value = SourceEnumValue.Mule });
    }

    [Fact]
    public void Should_be_enum()
    {
        _target.Value.ShouldBeOfType<TargetEnumValue>();
    }
}