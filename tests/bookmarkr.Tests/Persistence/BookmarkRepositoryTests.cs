using bookmarkr.Persistence;
using bookmarkr.Persistence.Abstractions;
using bookmarkr.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;


namespace bookmarkr.Tests.Persistence;

/// <summary>
/// Unit tests for the new BookmarkRepository implementation using Unit of Work pattern.
/// These tests validate the new architecture that follows DDD and SOLID principles.
/// </summary>
[TestFixture]
public class BookmarkRepositoryTests
{
    private BookmarkrDbContext _context = null!;
    private IBookmarkRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BookmarkrDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookmarkrDbContext(options);
        _repository = new BookmarkRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Test]
    public async Task GetByIdAsync_EntityExists_ReturnsEntity()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "Test", Url = "https://test.com", Category = "Testing" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(bookmark.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test"));
    }

    [Test]
    public async Task GetByIdAsync_EntityNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetByIdAsync_CancellationRequested_ThrowsOperationCancelledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _repository.GetByIdAsync(1, cts.Token));
    }

    #endregion

    #region GetAllAsync Tests

    [Test]
    public async Task GetAllAsync_MultipleEntities_ReturnsAllOrderedById()
    {
        // Arrange
        await _context.Bookmarks.AddRangeAsync(
            new Bookmark { Name = "Zebra", Url = "https://zebra.com" },
            new Bookmark { Name = "Apple", Url = "https://apple.com" },
            new Bookmark { Name = "Banana", Url = "https://banana.com" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].Id, Is.LessThan(result[1].Id));
    }

    [Test]
    public async Task GetAllAsync_NoEntities_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAllAsync_CancellationRequested_ThrowsOperationCancelledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _repository.GetAllAsync(cts.Token));
    }

    #endregion

    #region AddAsync Tests

    [Test]
    public async Task AddAsync_ValidEntity_AddsToContext()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "New", Url = "https://new.com" };

        // Act
        await _repository.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Assert
        var stored = await _context.Bookmarks.FirstOrDefaultAsync(b => b.Name == "New");
        Assert.That(stored, Is.Not.Null);
    }

    [Test]
    public void AddAsync_NullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _repository.AddAsync(null!));
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_TrackedEntity_MarksAsModified()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "Original", Url = "https://original.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var tracked = await _context.Bookmarks.FindAsync(bookmark.Id);
        tracked!.Name = "Modified";
        _repository.Update(tracked);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.Bookmarks.FindAsync(bookmark.Id);
        Assert.That(updated!.Name, Is.EqualTo("Modified"));
    }

    [Test]
    public void Update_NullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _repository.Update(null!));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_TrackedEntity_RemovesFromContext()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "ToDelete", Url = "https://delete.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var tracked = await _context.Bookmarks.FindAsync(bookmark.Id);
        _repository.Delete(tracked!);
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _context.Bookmarks.FindAsync(bookmark.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public void Delete_NullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _repository.Delete(null!));
    }

    #endregion

    #region GetByNameAsync Tests

    [Test]
    public async Task GetByNameAsync_NameExists_ReturnsBookmark()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "FindMe", Url = "https://findme.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("FindMe");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("FindMe"));
    }

    [Test]
    public async Task GetByNameAsync_CaseInsensitive_ReturnsBookmark()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "CaseSensitive", Url = "https://case.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("casesensitive");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("CaseSensitive"));
    }

    [Test]
    public async Task GetByNameAsync_NameNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistent");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetByNameAsync_NullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _repository.GetByNameAsync(null!));
    }

    [Test]
    public void GetByNameAsync_EmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _repository.GetByNameAsync(""));
    }

    [Test]
    public void GetByNameAsync_WhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _repository.GetByNameAsync("   "));
    }

    #endregion

    #region GetByUrlAsync Tests

    [Test]
    public async Task GetByUrlAsync_UrlExists_ReturnsBookmark()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "UrlTest", Url = "https://urltest.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUrlAsync("https://urltest.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url, Is.EqualTo("https://urltest.com"));
    }

    [Test]
    public async Task GetByUrlAsync_UrlNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByUrlAsync("https://nonexistent.com");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetByUrlAsync_NullUrl_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _repository.GetByUrlAsync(null!));
    }

    #endregion

    #region GetByCategoryAsync Tests

    [Test]
    public async Task GetByCategoryAsync_CategoryExists_ReturnsBookmarks()
    {
        // Arrange
        await _context.Bookmarks.AddRangeAsync(
            new Bookmark { Name = "Tech1", Url = "https://tech1.com", Category = "Tech" },
            new Bookmark { Name = "Tech2", Url = "https://tech2.com", Category = "Tech" },
            new Bookmark { Name = "Other", Url = "https://other.com", Category = "Other" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync("Tech");

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(b => b.Category == "Tech"), Is.True);
    }

    [Test]
    public async Task GetByCategoryAsync_CategoryNotExists_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetByCategoryAsync("NonExistent");

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetByCategoryAsync_CaseInsensitive_ReturnsBookmarks()
    {
        // Arrange
        await _context.Bookmarks.AddAsync(
            new Bookmark { Name = "Tech1", Url = "https://tech1.com", Category = "Technology" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync("technology");

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    #endregion

    #region ExistsByNameAsync Tests

    [Test]
    public async Task ExistsByNameAsync_NameExists_ReturnsTrue()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "Exists", Url = "https://exists.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByNameAsync("Exists");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsByNameAsync_NameNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsByNameAsync("DoesNotExist");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsByNameAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "ExistsCaseSensitive", Url = "https://existscase.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByNameAsync("EXISTSCASESENSITIVE");

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region ExistsByUrlAsync Tests

    [Test]
    public async Task ExistsByUrlAsync_UrlExists_ReturnsTrue()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "UrlExists", Url = "https://urlexists.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByUrlAsync("https://urlexists.com");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsByUrlAsync_UrlNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsByUrlAsync("https://doesnotexist.com");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsByUrlAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "UrlCase", Url = "https://UrlCaseSensitive.com" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByUrlAsync("https://urlcasesensitive.com");

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion
}
