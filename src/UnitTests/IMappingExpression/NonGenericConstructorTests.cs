namespace AutoMapper.UnitTests.Projection;
public class NonGenericConstructorTests : AutoMapperSpecBase
{
    private Dest[] _dest;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest()
        {
            
        }
        public Dest(int other)
        {
            Other = other;
        }

        public int Value { get; set; }
        [IgnoreMap]
        public int Other { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AddIgnoreMapAttribute();
        cfg.CreateMap(typeof (Source), typeof (Dest)).ConstructUsing(src => new Dest(((Source)src).Value + 10));
    });

    protected override void Because_of()
    {
        var values = new[]
        {
            new Source()
            {
                Value = 5
            }
        }.AsQueryable();

        _dest = values.ProjectTo<Dest>(Configuration).ToArray();
    }

    [Fact]
    public void Should_construct_correctly() => _dest[0].Other.ShouldBe(15);
}