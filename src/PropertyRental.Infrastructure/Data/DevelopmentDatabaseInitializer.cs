using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Infrastructure.Data;

public static class DevelopmentDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource;
        if (!IsLocalSqlServer(dataSource))
        {
            throw new InvalidOperationException(
                "Development startup migrations are allowed only for an explicitly local SQL Server.");
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await DevelopmentIdentitySeeder.SeedAsync(scope.ServiceProvider, configuration);
        
     
        await BogusDataSeeder.SeedAsync(dbContext);
    }

    private static bool IsLocalSqlServer(string dataSource)
    {
        var source = dataSource.Trim();
        return source.Equals(".", StringComparison.OrdinalIgnoreCase)
            || source.Equals("(local)", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith(".\\", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase)
            || source.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("localhost,", StringComparison.OrdinalIgnoreCase)
            || source.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("127.0.0.1,", StringComparison.OrdinalIgnoreCase)
            || source.Equals("::1", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("[::1],", StringComparison.OrdinalIgnoreCase);
    }
}