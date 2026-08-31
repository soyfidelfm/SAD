using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sad.Api.Data;

/// <summary>
/// Provides the EF Core DbContext for design-time commands such as
/// `dotnet ef database update` without executing the API startup validation.
/// </summary>
public sealed class SadDbContextFactory : IDesignTimeDbContextFactory<SadDbContext>
{
    public SadDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<SadDbContextFactory>(optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("SadDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing database connection string. Configure ConnectionStrings:SadDb using .NET User Secrets or the ConnectionStrings__SadDb environment variable.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<SadDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SadDbContext(optionsBuilder.Options);
    }
}
