using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bookmarkr.Persistence
{
    internal class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
    {
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
                .HasMaxLength(255);

            builder.Property(b => b.Url)
                .HasColumnName("URL")
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(b => b.Category)
                .HasColumnName("Category")
                .HasMaxLength(100);

            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("datetime('now')");

            builder.Property(b => b.UpdatedAt)
                .HasDefaultValueSql("datetime('now')");

            // Add unique constraints
            builder.HasIndex(b => b.Name)
                .IsUnique()
                .HasDatabaseName("IX_Bookmarks_Name_Unique");

            builder.HasIndex(b => b.Url)
                .IsUnique()
                .HasDatabaseName("IX_Bookmarks_Url_Unique");

            // Seed initial data with static DateTime values
            var staticDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed initial data
            builder.HasData(
                new Bookmark { Id = 1, Name = "First", Category = "Cars", Url = "https://www.lol.com", CreatedAt = staticDate, UpdatedAt = staticDate },
                new Bookmark { Id = 2, Name = "Second", Category = "Tech", Url = "https://www.second.com", CreatedAt = staticDate, UpdatedAt = staticDate },
                new Bookmark { Id = 3, Name = "Third", Category = "Tech", Url = "https://www.third.com", CreatedAt = staticDate, UpdatedAt = staticDate },
                new Bookmark {Id = 4, Name = "Fourth", Category = "Cooking", Url = "https://www.cooking.com", CreatedAt = staticDate, UpdatedAt = staticDate },
                new Bookmark {Id = 5, Name = "Fifth", Category = "SocialMedia", Url = "https://www.social.com", CreatedAt = staticDate, UpdatedAt = staticDate }
            );
        }
    }
}
