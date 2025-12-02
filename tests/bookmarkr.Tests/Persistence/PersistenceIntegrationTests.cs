using bookmarkr.Persistence;
using bookmarkr.Persistence.Extensions;
using bookmarkr.Persistence.Seeding;
using Microsoft.Extensions.DependencyInjection;
using NewIBookmarkRepository = bookmarkr.Persistence.Abstractions.IBookmarkRepository;
using NewIUnitOfWork = bookmarkr.Persistence.Abstractions.IUnitOfWork;

namespace bookmarkr.Tests.Persistence;

[TestFixture]
public class PersistenceIntegrationTests
{
    private ServiceProvider _serviceProvider = null!;
    private string _databaseName = null!;

    [SetUp]
    public void Setup()
    {
        _databaseName = $"IntegrationTest_{Guid.NewGuid()}.db";

        var services = new ServiceCollection();
        services.AddPersistenceServices($"Data Source={_databaseName}");

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookmarkrDbContext>();
        context.Database.EnsureCreated();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _serviceProvider.DisposeAsync();

        // Small delay to ensure file handles are released
        await Task.Delay(50);

        if (File.Exists(_databaseName))
        {
            try
            {
                File.Delete(_databaseName);
            }
            catch (IOException)
            {
                // File might still be locked, ignore cleanup failure in tests
            }
        }
    }

    [Test]
    public async Task AddAndRetrieveBookmark_ValidData_RoundTripsCorrectly()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();

        var bookmark = new Bookmark
        {
            Name = "Integration Test",
            Url = "https://integration.test.com",
            Category = "Testing"
        };

        await unitOfWork.Bookmarks.AddAsync(bookmark);
        await unitOfWork.SaveChangesAsync();

        var retrieved = await unitOfWork.Bookmarks.GetByNameAsync("Integration Test");

        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("Integration Test"));
        Assert.That(retrieved.Url, Is.EqualTo("https://integration.test.com"));
        Assert.That(retrieved.Category, Is.EqualTo("Testing"));
    }

    [Test]
    public async Task MultipleOperations_SingleSaveChanges_AllPersisted()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();

        await unitOfWork.Bookmarks.AddAsync(new Bookmark { Name = "Batch1", Url = "https://batch1.com" });
        await unitOfWork.Bookmarks.AddAsync(new Bookmark { Name = "Batch2", Url = "https://batch2.com" });
        await unitOfWork.Bookmarks.AddAsync(new Bookmark { Name = "Batch3", Url = "https://batch3.com" });

        await unitOfWork.SaveChangesAsync();

        var all = await unitOfWork.Bookmarks.GetAllAsync();
        Assert.That(all, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task ScopedContexts_IsolatedOperations_NoConflicts()
    {
        using (var scope1 = _serviceProvider.CreateScope())
        {
            var uow1 = scope1.ServiceProvider.GetRequiredService<NewIUnitOfWork>();
            await uow1.Bookmarks.AddAsync(new Bookmark { Name = "Scope1", Url = "https://scope1.com" });
            await uow1.SaveChangesAsync();
        }

        using (var scope2 = _serviceProvider.CreateScope())
        {
            var uow2 = scope2.ServiceProvider.GetRequiredService<NewIUnitOfWork>();
            await uow2.Bookmarks.AddAsync(new Bookmark { Name = "Scope2", Url = "https://scope2.com" });
            await uow2.SaveChangesAsync();
        }

        using var verifyScope = _serviceProvider.CreateScope();
        var verifyUow = verifyScope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();
        var all = await verifyUow.Bookmarks.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task DataSeeder_EmptyDatabase_SeedsData()
    {
        using var scope = _serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

        await seeder.SeedAsync();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();
        var all = await unitOfWork.Bookmarks.GetAllAsync();

        Assert.That(all, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task DataSeeder_ExistingData_DoesNotDuplicate()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();
            await unitOfWork.Bookmarks.AddAsync(new Bookmark { Name = "Existing", Url = "https://existing.com" });
            await unitOfWork.SaveChangesAsync();
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
        }

        using var verifyScope = _serviceProvider.CreateScope();
        var verifyUow = verifyScope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();
        var all = await verifyUow.Bookmarks.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(1), "Seeder should not add data when database is not empty");
    }

    [Test]
    public void DependencyInjection_ResolvesCorrectTypes()
    {
        using var scope = _serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();
        var repository = scope.ServiceProvider.GetRequiredService<NewIBookmarkRepository>();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

        Assert.That(unitOfWork, Is.Not.Null);
        Assert.That(repository, Is.Not.Null);
        Assert.That(seeder, Is.Not.Null);
    }

    [Test]
    public async Task CaseInsensitiveSearch_SQLiteCollation_Works()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<NewIUnitOfWork>();

        await unitOfWork.Bookmarks.AddAsync(new Bookmark
        {
            Name = "MixedCaseBookmark",
            Url = "https://MIXEDCASE.COM",
            Category = "TechCategory"
        });
        await unitOfWork.SaveChangesAsync();

        var byName = await unitOfWork.Bookmarks.GetByNameAsync("mixedcasebookmark");
        var byUrl = await unitOfWork.Bookmarks.GetByUrlAsync("https://mixedcase.com");
        var byCategory = await unitOfWork.Bookmarks.GetByCategoryAsync("techcategory");

        Assert.That(byName, Is.Not.Null, "Name search should be case-insensitive");
        Assert.That(byUrl, Is.Not.Null, "URL search should be case-insensitive");
        Assert.That(byCategory, Has.Count.EqualTo(1), "Category search should be case-insensitive");
    }
}
