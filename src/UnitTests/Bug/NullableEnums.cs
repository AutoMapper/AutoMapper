namespace AutoMapper.UnitTests.Bug;

public class NullableEnums : AutoMapperSpecBase
{
    public class Src { public EnumType? A { get; set; } }
    public class Dst { public EnumType? A { get; set; } }

    public enum EnumType { One, Two }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Src, Dst>();
    });

    [Fact]
    public void TestNullableEnum()
    {
        var d = Mapper.Map(new Src { A = null }, new Dst { A = EnumType.One });

        d.A.ShouldBeNull();
    } 
}