using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace AutoMapper.IntegrationTests;

public abstract class LocalDbContext : DbContext
{
    private readonly string _localDbVersion = "mssqllocaldb";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var baseConnectionString = @"Integrated Security=True; MultipleActiveResultSets=True;";

        var connectionString = string.Format(
            CultureInfo.InvariantCulture,
            @"Data Source=(localdb)\{1};{0};Database={2}",
            baseConnectionString,
            _localDbVersion,
            GetType().FullName);

        optionsBuilder.UseSqlServer(connectionString);
    }
}