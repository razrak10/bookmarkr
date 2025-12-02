namespace bookmarkr.Persistence.Abstractions;

/// <summary>
/// Repository interface for managing <see cref="Bookmark"/> entities with domain-specific query operations.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IRepository{T}"/> with bookmark-specific query methods
/// that encapsulate common lookup patterns. All methods use case-insensitive matching
/// for string comparisons.
/// </remarks>
public interface IBookmarkRepository : IRepository<Bookmark>
{
    /// <summary>
    /// Finds a bookmark by its exact name (case-insensitive).
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The bookmark if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    Task<Bookmark?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a bookmark by its URL (case-insensitive).
    /// </summary>
    /// <param name="url">The URL to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The bookmark if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is null or whitespace.</exception>
    Task<Bookmark?> GetByUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all bookmarks belonging to a specific category (case-insensitive).
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of bookmarks in the specified category.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="category"/> is null or whitespace.</exception>
    Task<IReadOnlyList<Bookmark>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a bookmark with the specified name already exists.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if a bookmark with the name exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a bookmark with the specified URL already exists.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if a bookmark with the URL exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken = default);
}
