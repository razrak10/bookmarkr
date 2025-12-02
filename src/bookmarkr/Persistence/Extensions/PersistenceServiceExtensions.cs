using bookmarkr.Persistence.Abstractions;
using bookmarkr.Persistence.Repositories;
using bookmarkr.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace bookmarkr.Persistence.Extensions;

/// <summary>
/// Extension methods for configuring persistence services in the dependency injection container.
/// </summary>
public static class PersistenceServiceExtensions
{
    /// <summary>
    /// Adds persistence services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <remarks>
    /// This method registers:
    /// <list type="bullet">
    /// <item><description><see cref="BookmarkrDbContext"/> as a scoped service</description></item>
    /// <item><description><see cref="IUnitOfWork"/> as a scoped service</description></item>
    /// <item><description><see cref="IBookmarkRepository"/> as a scoped service</description></item>
    /// <item><description><see cref="IDataSeeder"/> as a transient service</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddPersistenceServices("Data Source=bookmarks.db");
    /// </code>
    /// </example>
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<BookmarkrDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddTransient<IDataSeeder, BookmarkDataSeeder>();

        return services;
    }

    /// <summary>
    /// Seeds the database with initial data if running in development mode.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve services from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous seeding operation.</returns>
    /// <remarks>
    /// This method should be called during application startup after database migration.
    /// It creates a new scope to ensure proper service lifetime management.
    /// </remarks>
    public static async Task SeedDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
