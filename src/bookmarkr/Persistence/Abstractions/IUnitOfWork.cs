namespace bookmarkr.Persistence.Abstractions;

/// <summary>
/// Defines the Unit of Work pattern interface for managing transactional boundaries.
/// </summary>
/// <remarks>
/// The Unit of Work pattern maintains a list of objects affected by a business transaction
/// and coordinates the writing out of changes. This interface provides access to repositories
/// and a single point to commit all changes atomically.
/// <para>
/// Usage example:
/// <code>
/// await using var unitOfWork = serviceProvider.GetRequiredService&lt;IUnitOfWork&gt;();
/// await unitOfWork.Bookmarks.AddAsync(newBookmark);
/// await unitOfWork.SaveChangesAsync();
/// </code>
/// </para>
/// </remarks>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the bookmark repository for managing bookmark entities.
    /// </summary>
    /// <value>The bookmark repository instance.</value>
    IBookmarkRepository Bookmarks { get; }

    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The number of entities written to the data store.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
