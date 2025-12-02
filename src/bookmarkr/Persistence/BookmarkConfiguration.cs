using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bookmarkr.Persistence;

/// <summary>
/// Configures the entity mapping for <see cref="Bookmark"/> entities.
/// </summary>
/// <remarks>
/// This configuration defines the database schema for the Bookmarks table,
/// including column mappings, constraints, and indexes. String comparisons
/// use SQLite's NOCASE collation for case-insensitive matching.
/// </remarks>
internal sealed class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.ToTable("Bookmarks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("BookmarkId")
            .IsRequired();

        builder.Property(b => b.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(255)
            .UseCollation("NOCASE");

        builder.Property(b => b.Url)
            .HasColumnName("URL")
            .IsRequired()
            .HasMaxLength(2048)
            .UseCollation("NOCASE");

        builder.Property(b => b.Category)
            .HasColumnName("Category")
            .HasMaxLength(100)
            .UseCollation("NOCASE");

        builder.Property(b => b.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(b => b.UpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasIndex(b => b.Name)
            .IsUnique()
            .HasDatabaseName("IX_Bookmarks_Name_Unique");

        builder.HasIndex(b => b.Url)
            .IsUnique()
            .HasDatabaseName("IX_Bookmarks_Url_Unique");
    }
}
