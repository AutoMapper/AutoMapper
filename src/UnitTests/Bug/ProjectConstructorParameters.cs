namespace AutoMapper.UnitTests.Bug;

public class ProjectConstructorParameters : AutoMapperSpecBase
{
    SourceDto _dest;
    const int SomeValue = 15;

    public class Inner
    {
        public int Member { get; set; }
    }

    public class Source
    {
        public int Value { get; set; }
        public Inner Inner { get; set; }
    }

    public class SourceDto
    {
        private int _value;

        public SourceDto(int innerMember)
        {
            _value = innerMember;
        }

        public int Value
        {
            get { return _value; }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, SourceDto>();
    });

    protected override void Because_of()
    {
        var source = new Source { Inner = new Inner { Member = SomeValue } };
        //_dest = Mapper.Map<Source, SourceDto>(source);
        _dest = new[] { source }.AsQueryable().ProjectTo<SourceDto>(Configuration).First();
    }

    [Fact]
    public void Should_project_constructor_parameter_mappings()
    {
        _dest.Value.ShouldBe(SomeValue);
    }
}