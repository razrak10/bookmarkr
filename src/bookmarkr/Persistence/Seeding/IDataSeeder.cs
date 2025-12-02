namespace bookmarkr.Persistence.Seeding;

/// <summary>
/// Defines a contract for seeding data into the application's data store.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for populating initial or test data.
/// Data seeding is typically performed during application startup in development environments.
/// </remarks>
public interface IDataSeeder
{
    /// <summary>
    /// Seeds data into the data store asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous seeding operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
