using Microsoft.EntityFrameworkCore;
using bookmarkr.Persistence.Abstractions;

namespace bookmarkr.Persistence.Repositories;

/// <summary>
/// Abstract base class providing common repository operations for Entity Framework Core.
/// </summary>
/// <typeparam name="T">The entity type this repository manages.</typeparam>
/// <remarks>
/// This class implements the generic <see cref="IRepository{T}"/> interface and provides
/// standard CRUD operations using Entity Framework Core. Derived classes can add
/// entity-specific query methods while reusing the common functionality.
/// <para>
/// Note: This repository does not call <c>SaveChanges</c> - that responsibility
/// belongs to <see cref="IUnitOfWork"/> to ensure proper transactional boundaries.
/// </para>
/// </remarks>
public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    /// <summary>
    /// The database context used for data access operations.
    /// </summary>
    protected readonly BookmarkrDbContext Context;

    /// <summary>
    /// The DbSet for the entity type managed by this repository.
    /// </summary>
    protected readonly DbSet<T> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{T}"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    protected RepositoryBase(BookmarkrDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
        DbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Update(entity);
    }

    /// <inheritdoc />
    public virtual void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
    }
}
