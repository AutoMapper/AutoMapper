using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AutoMapper.IntegrationTests;

public class CreateDatabaseIfNotExists<TContext> : DropCreateDatabaseAlways<TContext>
    where TContext : DbContext, new()
{

}

public class DropCreateDatabaseAlways<TContext> where TContext : DbContext, new()
{
    protected virtual void Seed(TContext context)
    {

    }

    public async Task Migrate()
    {
        await using var context = new TContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        Seed(context);

        await context.SaveChangesAsync();
    }
}