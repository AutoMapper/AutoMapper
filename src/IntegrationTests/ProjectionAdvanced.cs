namespace AutoMapper.IntegrationTests;
public class ProjectionAdvanced : IntegrationTest<ProjectionAdvanced.Initializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateProjection<Entity, Dto>().Advanced().ForAllMembers(o=>o.Ignore()));
    [Fact]
    public void Should_work()
    {
        using var context = new Context();
        var dto = ProjectTo<Dto>(context.Entities).Single();
        dto.Id.ShouldBe(0);
        dto.Name.ShouldBeNull();
        dto.Value.ShouldBeNull();
    }
    public class Initializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context) => context.Add(new Entity { Name = "name", Value = "value" });
    }
    public class Context : LocalDbContext
    {
        public DbSet<Entity> Entities { get; set; }
    }
    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class Dto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}