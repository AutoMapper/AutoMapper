using Microsoft.EntityFrameworkCore;

namespace AutoMapper.IntegrationTests;

public abstract class LocalDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(
        @$"Data Source=(localdb)\mssqllocaldb;Integrated Security=True;MultipleActiveResultSets=True;Database={GetType()};Connection Timeout=160");
}