using bookmarkr.Persistence.Abstractions;
using bookmarkr.Persistence.Repositories;

namespace bookmarkr.Persistence.UnitOfWork;

/// <summary>
/// Implements the Unit of Work pattern to coordinate repository operations and manage transactions.
/// </summary>
/// <remarks>
/// This class provides a single point for committing all changes made through repositories
/// within a single transaction. Repository instances are lazily initialized to avoid
/// unnecessary object creation.
/// <para>
/// The Unit of Work ensures that multiple repository operations can be batched
/// and committed atomically, maintaining data consistency.
/// </para>
/// </remarks>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BookmarkrDbContext _context;
    private IBookmarkRepository? _bookmarkRepository;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context to use for this unit of work.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public UnitOfWork(BookmarkrDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    /// <remarks>
    /// The repository instance is lazily initialized on first access
    /// and reused for subsequent calls within the same unit of work.
    /// </remarks>
    public IBookmarkRepository Bookmarks => _bookmarkRepository ??= new BookmarkRepository(_context);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _context.DisposeAsync();
            _disposed = true;
        }
    }
}
