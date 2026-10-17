using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Uvidik.Infrastructure.Persistence;

namespace Uvidik.Infrastructure.IntegrationTests;

public class MigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Migrate_AppliesCleanly_AgainstRealPostgres()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Assert.Contains(appliedMigrations, m => m.Contains("InitialCreate"));
    }

    [Fact]
    public async Task Migrate_IsIdempotent_WhenRunTwice()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();
        await context.Database.MigrateAsync();

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Assert.Single(appliedMigrations);
    }

    private UvidikDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UvidikDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new UvidikDbContext(options);
    }
}
