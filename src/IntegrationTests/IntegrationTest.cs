namespace AutoMapper.IntegrationTests;

public abstract class IntegrationTest<TInitializer> : AutoMapperSpecBase, IAsyncLifetime where TInitializer : IInitializer, new()
{
    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
    Task IAsyncLifetime.InitializeAsync() => new TInitializer().Migrate();
}
public interface IInitializer
{
    Task Migrate();
}
public class DropCreateDatabaseAlways<TContext> : IInitializer where TContext : DbContext, new()
{
    protected virtual void Seed(TContext context){}
    public async Task Migrate()
    {
        await using var context = new TContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        Seed(context);

        await context.SaveChangesAsync();
    }
}