namespace AutoMapper.IntegrationTests.Inheritance;

public class QueryableInterfaceInheritanceIssue : IntegrationTest<QueryableInterfaceInheritanceIssue.DatabaseInitializer>
{
    QueryableDto[] _result;

    public interface IBaseQueryableInterface
    {
        string Id { get; set; }
    }

    public interface IQueryableInterface : IBaseQueryableInterface
    {
    }

    public class QueryableInterfaceImpl : IQueryableInterface
    {
        public string Id { get; set; }
    }

    public class QueryableDto
    {
        public string Id { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Entities.AddRange(new[] { new QueryableInterfaceImpl { Id = "One" }, new QueryableInterfaceImpl { Id = "Two" }});
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<QueryableInterfaceImpl> Entities { get; set; }
    }

    [Fact]
    public void QueryableShouldMapSpecifiedBaseInterfaceMember()
    {
        using (var context = new ClientContext())
        {
            _result = ProjectTo<QueryableDto>(context.Entities).ToArray();
        }
        _result.FirstOrDefault(dto => dto.Id == "One").ShouldNotBeNull();
        _result.FirstOrDefault(dto => dto.Id == "Two").ShouldNotBeNull();
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<IQueryableInterface, QueryableDto>());
}