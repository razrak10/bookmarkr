using BenchmarkDotNet.Attributes;
using bookmarkr.Commands;
using bookmarkr.Commands.Export;
using bookmarkr.Persistence;
using bookmarkr.Service;
using bookmarkr.ServiceAgent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace bookmarkr.Benchmarks
{
    [MemoryDiagnoser]
    public class BookmarkrBenchmarks
    {
        #region Properties
        private IBookmarkService? _service;
        private ServiceProvider _serviceProvider = null!;
        private List<Bookmark> _testBookmarks = null!;
        private string _tempDirectory = null!;

        #endregion

        #region GlobalSetup
        [GlobalSetup]
        public void BenchmarksGlobalSetup()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "BookmarkrBenchmarks", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);

            var services = new ServiceCollection();

            services.AddDbContext<BookmarkrDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
            });

            services.AddScoped<IBookmarkRepository, BookmarkRepository>();

            services.AddSingleton(Mock.Of<IBookmarkrLookupServiceAgent>());
            services.AddScoped<IBookmarkService, BookmarkService>();

            _serviceProvider = services.BuildServiceProvider();
            _service = _serviceProvider.GetRequiredService<IBookmarkService>();

            SeedDatabase().GetAwaiter().GetResult();
        }

        private async Task SeedDatabase()
        {
            _testBookmarks = new List<Bookmark>
            {
                new() { Name = "Google", Url = "https://www.google.com", Category = "Search" },
                new() { Name = "GitHub", Url = "https://www.github.com", Category = "Development" },
                new() { Name = "Stack Overflow", Url = "https://stackoverflow.com", Category = "Development" },
                new() { Name = "Microsoft Docs", Url = "https://docs.microsoft.com", Category = "Documentation" },
                new() { Name = "YouTube", Url = "https://www.youtube.com", Category = "Entertainment" },
                new() { Name = "Netflix", Url = "https://www.netflix.com", Category = "Entertainment" },
                new() { Name = "Amazon", Url = "https://www.amazon.com", Category = "Shopping" },
                new() { Name = "Wikipedia", Url = "https://www.wikipedia.org", Category = "Reference" },
                new() { Name = "Reddit", Url = "https://www.reddit.com", Category = "Social" },
                new() { Name = "Twitter", Url = "https://www.twitter.com", Category = "Social" }
            };

            // Add more bookmarks for stress testing
            for (int i = 0; i < 100; i++)
            {
                _testBookmarks.Add(new Bookmark
                {
                    Name = $"Test Bookmark {i}",
                    Url = $"https://example{i}.com",
                    Category = $"Category{i % 10}"
                });
            }

            // Add bookmarks to the database
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBookmarkRepository>();

            foreach (var bookmark in _testBookmarks)
            {
                await repository.AddAsync(bookmark);
            }
        }

        #endregion

        [GlobalCleanup]
        public void BenchmarksGlobalCleanup()
        {
            _serviceProvider?.Dispose();

            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Benchmark]
        public async Task ExportBookmarks()
        {
            var exportCmdHandler = new ExportCommandHandler(_service!);
            var exportCmd = new ExportCommand(exportCmdHandler!, "export", "Exports all bookmarks to a file");

            var exportArgs = new string[] { "--file", "bookmarksbench.json" };
            await exportCmd.Parse(exportArgs).InvokeAsync();
        }
    }
}
