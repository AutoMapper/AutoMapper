using System.Linq;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.Inheritance;

public class QueryableInterfaceInheritanceIssue : AutoMapperSpecBase, IAsyncLifetime
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

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Entities.AddRange(new[] { new QueryableInterfaceImpl { Id = "One" }, new QueryableInterfaceImpl { Id = "Two" }});
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<QueryableInterfaceImpl> Entities { get; set; }
    }

    [Fact]
    public void QueryableShouldMapSpecifiedBaseInterfaceMember()
    {
        _result.FirstOrDefault(dto => dto.Id == "One").ShouldNotBeNull();
        _result.FirstOrDefault(dto => dto.Id == "Two").ShouldNotBeNull();
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<IQueryableInterface, QueryableDto>());

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();

        using (var context = new ClientContext())
        {
            _result = ProjectTo<QueryableDto>(context.Entities).ToArray();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}