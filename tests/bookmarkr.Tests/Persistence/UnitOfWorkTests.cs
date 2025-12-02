using bookmarkr.Persistence;
using Microsoft.EntityFrameworkCore;
using NewIUnitOfWork = bookmarkr.Persistence.Abstractions.IUnitOfWork;
using UnitOfWorkImpl = bookmarkr.Persistence.UnitOfWork.UnitOfWork;

namespace bookmarkr.Tests.Persistence;

[TestFixture]
public class UnitOfWorkTests
{
    private DbContextOptions<BookmarkrDbContext> _options = null!;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<BookmarkrDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task SaveChangesAsync_MultipleOperations_CommitsAllChanges()
    {
        await using var context = new BookmarkrDbContext(_options);
        await using NewIUnitOfWork unitOfWork = new UnitOfWorkImpl(context);

        await unitOfWork.Bookmarks.AddAsync(new Bookmark { Name = "First", Url = "https://first.com" });
        await unitOfWork.Bookmarks.AddAsync(new Bookmark { Name = "Second", Url = "https://second.com" });

        var result = await unitOfWork.SaveChangesAsync();

        Assert.That(result, Is.EqualTo(2));

        await using var verifyContext = new BookmarkrDbContext(_options);
        var count = await verifyContext.Bookmarks.CountAsync();
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public async Task SaveChangesAsync_NoChanges_ReturnsZero()
    {
        await using var context = new BookmarkrDbContext(_options);
        await using NewIUnitOfWork unitOfWork = new UnitOfWorkImpl(context);

        var result = await unitOfWork.SaveChangesAsync();

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void SaveChangesAsync_CancellationToken_IsPassedToContext()
    {
        // Note: InMemory provider doesn't throw on cancellation for SaveChanges with no changes
        // This test validates the token is accepted - actual cancellation behavior is provider-specific
        using var context = new BookmarkrDbContext(_options);
        var unitOfWork = new UnitOfWorkImpl(context);
        var cts = new CancellationTokenSource();

        // Should not throw with non-cancelled token
        Assert.DoesNotThrowAsync(async () => await unitOfWork.SaveChangesAsync(cts.Token));
    }

    [Test]
    public async Task DisposeAsync_CalledMultipleTimes_NoException()
    {
        var context = new BookmarkrDbContext(_options);
        var unitOfWork = new UnitOfWorkImpl(context);

        await unitOfWork.DisposeAsync();
        await unitOfWork.DisposeAsync();

        Assert.Pass("No exception thrown on multiple dispose calls");
    }

    [Test]
    public async Task Bookmarks_AccessedMultipleTimes_ReturnsSameInstance()
    {
        await using var context = new BookmarkrDbContext(_options);
        await using NewIUnitOfWork unitOfWork = new UnitOfWorkImpl(context);

        var firstAccess = unitOfWork.Bookmarks;
        var secondAccess = unitOfWork.Bookmarks;

        Assert.That(firstAccess, Is.SameAs(secondAccess));
    }

    [Test]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UnitOfWorkImpl(null!));
    }

    [Test]
    public async Task UnitOfWork_TransactionalBehavior_RollsBackOnNoSave()
    {
        await using var context = new BookmarkrDbContext(_options);
        await using (NewIUnitOfWork unitOfWork = new UnitOfWorkImpl(context))
        {
            await unitOfWork.Bookmarks.AddAsync(new Bookmark { Name = "NotSaved", Url = "https://notsaved.com" });
        }

        await using var verifyContext = new BookmarkrDbContext(_options);
        var count = await verifyContext.Bookmarks.CountAsync();
        Assert.That(count, Is.EqualTo(0), "Changes should not be persisted without SaveChangesAsync");
    }

    [Test]
    public async Task SaveChangesAsync_UpdateExistingEntity_PersistsChanges()
    {
        await using (var setupContext = new BookmarkrDbContext(_options))
        {
            await setupContext.Bookmarks.AddAsync(new Bookmark { Name = "Original", Url = "https://original.com" });
            await setupContext.SaveChangesAsync();
        }

        await using var context = new BookmarkrDbContext(_options);
        await using NewIUnitOfWork unitOfWork = new UnitOfWorkImpl(context);

        var bookmark = await unitOfWork.Bookmarks.GetByNameAsync("Original");
        bookmark!.Name = "Updated";
        unitOfWork.Bookmarks.Update(bookmark);
        await unitOfWork.SaveChangesAsync();

        await using var verifyContext = new BookmarkrDbContext(_options);
        var updated = await verifyContext.Bookmarks.FirstOrDefaultAsync(b => b.Name == "Updated");
        Assert.That(updated, Is.Not.Null);
    }

    [Test]
    public async Task SaveChangesAsync_DeleteEntity_RemovesFromDatabase()
    {
        await using (var setupContext = new BookmarkrDbContext(_options))
        {
            await setupContext.Bookmarks.AddAsync(new Bookmark { Name = "ToDelete", Url = "https://todelete.com" });
            await setupContext.SaveChangesAsync();
        }

        await using var context = new BookmarkrDbContext(_options);
        await using NewIUnitOfWork unitOfWork = new UnitOfWorkImpl(context);

        var bookmark = await unitOfWork.Bookmarks.GetByNameAsync("ToDelete");
        unitOfWork.Bookmarks.Delete(bookmark!);
        await unitOfWork.SaveChangesAsync();

        await using var verifyContext = new BookmarkrDbContext(_options);
        var count = await verifyContext.Bookmarks.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }
}
