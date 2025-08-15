using Microsoft.EntityFrameworkCore;

namespace bookmarkr.Persistence
{
    /// <summary>
    /// Represents the database context for the Bookmarkr application, providing access to the application's data
    /// models.
    /// </summary>
    /// <remarks>This class is used to interact with the database using Entity Framework Core. It provides
    /// methods for querying and saving data. Configure the database connection and model relationships in the derived
    /// class or through the application's configuration.
    /// https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ </remarks>
    internal class BookmarkrDbContext : DbContext
    {
        public BookmarkrDbContext(DbContextOptions<BookmarkrDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bookmark> Bookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BookmarkConfiguration());
        }

    }
}
