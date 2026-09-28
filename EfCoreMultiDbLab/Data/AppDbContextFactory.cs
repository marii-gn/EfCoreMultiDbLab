using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EfCoreMultiDbLab.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var provider = configuration["DatabaseProvider"] ?? "Sqlite";
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            builder.UseSqlite(configuration.GetConnectionString("Sqlite"));
        }
        else if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            builder.UseNpgsql(configuration.GetConnectionString("PostgreSql"));
        }

        return new AppDbContext(builder.Options);
    }
}
