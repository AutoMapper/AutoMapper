namespace AutoMapper.UnitTests;
public class BuildExecutionPlan : AutoMapperSpecBase
{
    Model _source;
    Dto _destination;
    public class Model
    {
        public Guid? Id { get; set; }
        public Guid? FooId { get; set; }
        public string FullDescription { get; set; }
        public string ShortDescription { get; set; }
        public DateTime Date { get; set; }
        public int? IntValue { get; set; }
    }
    public class Dto
    {
        public Guid? Id { get; set; }
        public string FooId { get; set; }
        public string FullDescription { get; set; }
        public string ShortDescription { get; set; }
        public DateTime Date { get; set; }
        public int IntValue { get; set; }
        public string CompanyName { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<Model, Dto>().ForMember(d => d.CompanyName, o => o.Ignore());
    });
    protected override void Because_of()
    {
        _source = new Model
        {
            Id = Guid.NewGuid(),
            FooId = Guid.NewGuid(),
            ShortDescription = "Yoyodyne Foo",
            FullDescription = "Deluxe Foo Manufactured by Yoyodyne, Inc.",
            Date = DateTime.Now,
            IntValue = 13,
        };
        var plan = Configuration.BuildExecutionPlan(typeof(Model), typeof(Dto));
        _destination = ((Func<Model, Dto, ResolutionContext, Dto>)plan.Compile())(_source, null, null);
    }
    [Fact]
    public void Should_build_the_execution_plan()
    {
        _destination.Id.ShouldBe(_source.Id);
        _destination.FooId.ShouldBe(_source.FooId.ToString());
        _destination.ShortDescription.ShouldBe(_source.ShortDescription);
        _destination.FullDescription.ShouldBe(_source.FullDescription);
        _destination.Date.ShouldBe(_source.Date);
        _destination.IntValue.ShouldBe(_source.IntValue.Value);
    }
}
public class When_reusing_the_execution_plan_inner_map : AutoMapperSpecBase
{
    class Source
    {
        public Inner Inner { get; set; }
    }
    class Destination
    {
        public Inner Inner { get; set; }
    }
    class Inner { }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.AllowNullDestinationValues = false;
        c.CreateMap<Inner, Inner>();
        c.CreateMap<Source, Destination>().ForAllMembers(o =>
        {
            o.AllowNull();
            o.MapAtRuntime();
        });
    });
    [Fact]
    public void Should_consider_per_member_settings()
    {
        Mapper.Map<Inner, Inner>(null).ShouldNotBeNull();
        var destination = Map<Destination>(new Source());
        destination.Inner.ShouldBeNull();
    }
}
public class AllowNullWithMapAtRuntime : AutoMapperSpecBase
{
    class Source
    {
        public Inner Inner { get; set; }
    }
    class Destination
    {
        public Inner Inner { get; set; }
    }
    class Inner { }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.AllowNullDestinationValues = false;
        c.CreateMap<Inner, Inner>();
        c.CreateMap<Source, Destination>().ForAllMembers(o =>
        {
            o.AllowNull();
            o.MapAtRuntime();
        });
    });
    [Fact]
    public void Should_consider_per_member_settings()
    {
        var destination = Map<Destination>(new Source());
        destination.Inner.ShouldBeNull();
    }
}
public class When_reusing_the_execution_plan : AutoMapperSpecBase
{
    class Source
    {
        public int[] Ints { get; set; }
        public string String { get; set; }
    }
    class Destination
    {
        public int[] Ints { get; set; }
        public string String { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.AllowNullDestinationValues = false;
        c.CreateMap<Source, Destination>().ForAllMembers(o =>
        {
            o.AllowNull();
            o.MapAtRuntime();
        });
    });
    [Fact]
    public void Should_consider_per_member_settings()
    {
        Mapper.Map<string, string>(null).Length.ShouldBe(0);
        Mapper.Map<int[], int[]>(null).Length.ShouldBe(0);
        var destination = Map<Destination>(new Source());
        destination.Ints.ShouldBeNull();
        destination.String.ShouldBeNull();
    }
}
public class When_reusing_the_execution_plan_existing_destination : AutoMapperSpecBase
{
    class Source
    {
        public int[] Ints { get; set; }
    }
    class OtherSource
    {
        public int[] Ints { get; set; }
    }
    class Destination
    {
        public ICollection<int> Ints { get; set; } = new HashSet<int>();
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<OtherSource, Destination>().ForAllMembers(o => o.MapAtRuntime());
        c.CreateMap<Source, Destination>().ForAllMembers(o =>
        {
            o.UseDestinationValue();
            o.MapAtRuntime();
        });
    });
    [Fact]
    public void Should_consider_per_member_settings()
    {
        var ints = new[] { 1, 1, 1 };
        var destination = Map<Destination>(new OtherSource { Ints = ints });
        destination.Ints.ShouldBe(ints);
        destination = Map<Destination>(new Source { Ints = ints });
        destination.Ints.ShouldBe(new[] { 1 });
    }
}