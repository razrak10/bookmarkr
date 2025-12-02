using bookmarkr.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;


namespace bookmarkr.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="Bookmark"/> entities.
/// </summary>
/// <remarks>
/// This repository provides domain-specific query operations for bookmarks
/// in addition to the standard CRUD operations inherited from <see cref="RepositoryBase{T}"/>.
/// All string comparisons use case-insensitive matching via EF.Functions.Like or ToLower().
/// </remarks>
public sealed class BookmarkRepository : RepositoryBase<Bookmark>, IBookmarkRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookmarkRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public BookmarkRepository(BookmarkrDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<Bookmark>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Bookmark?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var lowerName = name.ToLowerInvariant();
        return await DbSet
            .FirstOrDefaultAsync(b => b.Name.ToLower() == lowerName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Bookmark?> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var lowerUrl = url.ToLowerInvariant();
        return await DbSet
            .FirstOrDefaultAsync(b => b.Url.ToLower() == lowerUrl, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Bookmark>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        var lowerCategory = category.ToLowerInvariant();
        return await DbSet
            .AsNoTracking()
            .Where(b => b.Category != null && b.Category.ToLower() == lowerCategory)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var lowerName = name.ToLowerInvariant();
        return await DbSet
            .AnyAsync(b => b.Name.ToLower() == lowerName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var lowerUrl = url.ToLowerInvariant();
        return await DbSet
            .AnyAsync(b => b.Url.ToLower() == lowerUrl, cancellationToken);
    }
}
