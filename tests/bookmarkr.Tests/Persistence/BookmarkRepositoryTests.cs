using bookmarkr.ExecutionResult;
using bookmarkr.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bookmarkr.Tests.Persistence;

[TestFixture]
public class BookmarkRepositoryTests
{
    private BookmarkrDbContext _context;
    private BookmarkRepository _repository;
    private List<Bookmark> _testBookmarks;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookmarkrDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookmarkrDbContext(options);
        _repository = new BookmarkRepository(_context);

        _testBookmarks = new List<Bookmark>
        {
            new Bookmark { Id = 1, Name = "Test Bookmark 1", Url = "https://test1.com", Category = "Tech" },
            new Bookmark { Id = 2, Name = "Test Bookmark 2", Url = "https://test2.com", Category = "Social" },
            new Bookmark { Id = 3, Name = "Test Bookmark 3", Url = "https://test3.com", Category = "Tech" }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public void Constructor_WithValidContext_ShouldCreateInstance()
    {
        // Arrange & Act
        var repository = new BookmarkRepository(_context);

        // Assert
        Assert.That(repository, Is.Not.Null);
        Assert.That(repository, Is.InstanceOf<IBookmarkRepository>());
    }

    [Test]
    public async Task AddAsync_WithValidBookmark_ShouldReturnSuccessResult()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "New Bookmark", Url = "https://new.com", Category = "Tech" };

        // Act
        var result = await _repository.AddAsync(bookmark);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(bookmark));
        Assert.That(bookmark.Id, Is.GreaterThan(0));

        var savedBookmark = await _context.Bookmarks.FirstOrDefaultAsync(b => b.Name == "New Bookmark");
        Assert.That(savedBookmark, Is.Not.Null);
        Assert.That(savedBookmark.Url, Is.EqualTo("https://new.com"));
    }

    [Test]
    public async Task DeleteAsync_WithExistingBookmark_ShouldReturnSuccessResult()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "To Delete", Url = "https://delete.com", Category = "Tech" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(bookmark);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(bookmark));

        var deletedBookmark = await _context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmark.Id);
        Assert.That(deletedBookmark, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingBookmark_ShouldReturnFailureResult()
    {
        // Arrange
        var bookmark = new Bookmark { Id = 999, Name = "Non Existing", Url = "https://nonexisting.com", Category = "Tech" };

        // Act
        var result = await _repository.DeleteAsync(bookmark);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Error occured while deleting bookmark from database"));
    }

    [Test]
    public async Task FindAllAsync_WithTrackingChanges_ShouldReturnOrderedBookmarks()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.FindAllAsync(true);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count(), Is.EqualTo(3));

        var bookmarks = result.Value.ToList();
        Assert.That(bookmarks[0].Id, Is.LessThanOrEqualTo(bookmarks[1].Id));
        Assert.That(bookmarks[1].Id, Is.LessThanOrEqualTo(bookmarks[2].Id));
    }

    [Test]
    public async Task FindAllAsync_WithoutTrackingChanges_ShouldReturnOrderedBookmarks()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.FindAllAsync(false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count(), Is.EqualTo(3));

        var bookmarks = result.Value.ToList();
        Assert.That(bookmarks[0].Id, Is.LessThanOrEqualTo(bookmarks[1].Id));
    }

    [Test]
    public async Task FindAllAsync_WithEmptyDatabase_ShouldReturnEmptyCollection()
    {
        // Act
        var result = await _repository.FindAllAsync(false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateAsync_WithValidBookmark_ShouldReturnSuccessResult()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "Original", Url = "https://original.com", Category = "Tech" };
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();

        bookmark.Name = "Updated";
        bookmark.Url = "https://updated.com";

        // Act
        var result = await _repository.UpdateAsync(bookmark);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Updated"));
        Assert.That(result.Value.Url, Is.EqualTo("https://updated.com"));

        var updatedBookmark = await _context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmark.Id);
        Assert.That(updatedBookmark.Name, Is.EqualTo("Updated"));
    }

    [Test]
    public async Task FindBookmarkById_WithExistingId_ShouldReturnBookmark()
    {
        // Arrange
        await SeedDatabase();
        var expectedBookmark = _testBookmarks[0];

        // Act
        var result = await _repository.FindBookmarkById(expectedBookmark.Id, false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Id, Is.EqualTo(expectedBookmark.Id));
        Assert.That(result.Value.Name, Is.EqualTo(expectedBookmark.Name));
    }

    [Test]
    public async Task FindBookmarkById_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.FindBookmarkById(999, false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public async Task FindBookmarkByName_WithExistingName_ShouldReturnBookmark()
    {
        // Arrange
        await SeedDatabase();
        var expectedBookmark = _testBookmarks[0];

        // Act
        var result = await _repository.FindBookmarkByName(expectedBookmark.Name, false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Name, Is.EqualTo(expectedBookmark.Name));
    }

    [Test]
    public async Task FindBookmarkByName_WithCaseInsensitiveName_ShouldReturnBookmark()
    {
        // Arrange
        await SeedDatabase();
        var expectedBookmark = _testBookmarks[0];

        // Act
        var result = await _repository.FindBookmarkByName(expectedBookmark.Name.ToUpper(), false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Name, Is.EqualTo(expectedBookmark.Name));
    }

    [Test]
    public async Task FindBookmarkByName_WithNonExistingName_ShouldReturnNull()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.FindBookmarkByName("Non Existing", false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public async Task FindBookmarkByUrl_WithExistingUrl_ShouldReturnBookmark()
    {
        // Arrange
        await SeedDatabase();
        var expectedBookmark = _testBookmarks[0];

        // Act
        var result = await _repository.FindBookmarkByUrl(expectedBookmark.Url, false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Url, Is.EqualTo(expectedBookmark.Url));
    }

    [Test]
    public async Task FindBookmarkByUrl_WithCaseInsensitiveUrl_ShouldReturnBookmark()
    {
        // Arrange
        await SeedDatabase();
        var expectedBookmark = _testBookmarks[0];

        // Act
        var result = await _repository.FindBookmarkByUrl(expectedBookmark.Url.ToUpper(), false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Url, Is.EqualTo(expectedBookmark.Url));
    }

    [Test]
    public async Task FindBookmarkByUrl_WithNonExistingUrl_ShouldReturnNull()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.FindBookmarkByUrl("https://nonexisting.com", false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public async Task FindBookmarksByCategory_WithExistingCategory_ShouldReturnBookmarks()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.FindBookmarksByCategory("Tech", false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count(), Is.EqualTo(2));
        Assert.That(result.Value.All(b => b.Category == "Tech"), Is.True);
    }

    [Test]
    public async Task FindBookmarksByCategory_WithNonExistingCategory_ShouldReturnEmptyCollection()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.FindBookmarksByCategory("NonExisting", false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task SaveChangesAsync_ShouldReturnNumberOfChanges()
    {
        // Arrange
        var bookmark = new Bookmark { Name = "Test Save", Url = "https://save.com", Category = "Tech" };
        await _context.Bookmarks.AddAsync(bookmark);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task SaveChangesAsync_WithNoChanges_ShouldReturnZero()
    {
        // Arrange
        await SeedDatabase();

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task ConcurrentOperations_ShouldHandleThreadSafety()
    {
        // Arrange
        var tasks = new List<Task<ExecutionResult<Bookmark>>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var bookmark = new Bookmark { Name = $"Concurrent {i}", Url = $"https://concurrent{i}.com", Category = "Tech" };
            tasks.Add(_repository.AddAsync(bookmark));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.That(results.All(r => r.IsSuccess), Is.True);

        var allBookmarks = await _repository.FindAllAsync(false);
        Assert.That(allBookmarks.Value.Count(), Is.EqualTo(10));
    }

    [Test]
    public async Task FindBookmarkById_WithTrackingChanges_ShouldReturnTrackedEntity()
    {
        // Arrange
        await SeedDatabase();
        var expectedBookmark = _testBookmarks[0];

        // Act
        var result = await _repository.FindBookmarkById(expectedBookmark.Id, true);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        // Verify tracking by checking EntityState
        var entry = _context.Entry(result.Value);
        Assert.That(entry.State, Is.EqualTo(EntityState.Unchanged));
    }

    [Test]
    public async Task FindBookmarkById_WithoutTrackingChanges_ShouldReturnUntrackedEntity()
    {
        // Arrange
        await SeedDatabase();
        var expectedBookmark = _testBookmarks[0];

        // Act
        var result = await _repository.FindBookmarkById(expectedBookmark.Id, false);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        // Verify no tracking by checking EntityState
        var entry = _context.Entry(result.Value);
        Assert.That(entry.State, Is.EqualTo(EntityState.Detached));
    }

    private async Task SeedDatabase()
    {
        await _context.Bookmarks.AddRangeAsync(_testBookmarks);
        await _context.SaveChangesAsync();
    }
}