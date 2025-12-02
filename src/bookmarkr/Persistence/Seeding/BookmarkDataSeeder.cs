using Microsoft.EntityFrameworkCore;

namespace bookmarkr.Persistence.Seeding;

/// <summary>
/// Seeds initial bookmark data for development and testing purposes.
/// </summary>
/// <remarks>
/// This seeder checks for existing data before inserting to prevent duplicate entries.
/// It should only be used in development environments.
/// </remarks>
public sealed class BookmarkDataSeeder : IDataSeeder
{
    private readonly BookmarkrDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookmarkDataSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context to use for seeding.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public BookmarkDataSeeder(BookmarkrDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Bookmarks.AnyAsync(cancellationToken))
        {
            return;
        }

        var staticDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var seedBookmarks = new List<Bookmark>
        {
            new() { Name = "First", Category = "Cars", Url = "https://www.lol.com", CreatedAt = staticDate, UpdatedAt = staticDate },
            new() { Name = "Second", Category = "Tech", Url = "https://www.second.com", CreatedAt = staticDate, UpdatedAt = staticDate },
            new() { Name = "Third", Category = "Tech", Url = "https://www.third.com", CreatedAt = staticDate, UpdatedAt = staticDate },
            new() { Name = "Fourth", Category = "Cooking", Url = "https://www.cooking.com", CreatedAt = staticDate, UpdatedAt = staticDate },
            new() { Name = "Fifth", Category = "SocialMedia", Url = "https://www.social.com", CreatedAt = staticDate, UpdatedAt = staticDate }
        };

        await _context.Bookmarks.AddRangeAsync(seedBookmarks, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
