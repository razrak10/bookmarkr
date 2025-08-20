using bookmarkr.Commands;
using bookmarkr.Commands.Category;
using bookmarkr.Commands.Export;
using bookmarkr.Commands.Import;
using bookmarkr.Commands.Interactive;
using bookmarkr.Commands.Link;
using bookmarkr.Commands.Link.Add;
using bookmarkr.Commands.Link.Remove;
using bookmarkr.Commands.Link.Update;
using bookmarkr.Commands.Show;
using bookmarkr.Commands.Sync;
using bookmarkr.Persistence;
using bookmarkr.Service;
using bookmarkr.ServiceAgent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.CommandLine;

namespace bookmarkr;

class Program
{
    private static IHost _host = null!;

    static async Task<int> Main(string[] args)
    {
        FreeSerilogLoggerOnShutdown();
        CreateLogger();

        IHost host = Host.CreateDefaultBuilder(args)
             .ConfigureServices((hostcontext, services) =>
             {
                 // Add Entity Framework
                 services.AddDbContext<BookmarkrDbContext>(options => options.UseSqlite("Data Source=bookmarks.db"));

                 services.AddScoped<IBookmarkRepository, BookmarkRepository>();

                 // Register BookmarkService
                 services.AddScoped<IBookmarkService, BookmarkService>();

                 // Register HttpClient
                 services.AddHttpClient();
                 services.AddHttpClient("bookmarkrSyncr", client =>
                 {
                     client.BaseAddress = new Uri("https://bookmarkrsyncr-api.azurewebsites.net");
                     client.DefaultRequestHeaders.Add("Accept", "application/json");
                     client.DefaultRequestHeaders.Add("User-Agent", "Bookmarkr");
                 });

                 // Register command handlers
                 services.AddTransient<RootCommandHandler>();
                 services.AddTransient<InteractiveCommandHandler>();
                 services.AddTransient<LinkCommandHandler>();
                 services.AddTransient<LinkAddCommandHandler>();
                 services.AddTransient<LinkRemoveCommandHandler>();
                 services.AddTransient<LinkUpdateCommandHandler>();
                 services.AddTransient<ExportCommandHandler>();
                 services.AddTransient<ImportCommandHandler>();
                 services.AddTransient<ShowCommandHandler>();
                 services.AddTransient<CategoryCommandHandler>();
                 services.AddTransient<ChangeCommandHandler>();
                 services.AddTransient<SyncCommandHandler>();

                 // Register service agent
                 services.AddScoped<IBookmarkrSyncrServiceAgent, BookmarkrSyncrServiceAgent>();
                 services.AddScoped<IBookmarkrLookupServiceAgent, BookmarkrLookupServiceAgent>();
             })
             .Build();

        _host = host;

        await EnsureDatabaseAsync();

        RootCommand rootCommand = new RootCommand("Bookmarkr is a bookmark manager provided as a CLI application");

        // Configure commands
        ConfigureCommands(rootCommand);

        return await rootCommand.Parse(args).InvokeAsync();
    }

    private static void ConfigureCommands(RootCommand rootCommand)
    {
        // Root command action - async
        rootCommand.SetAction((parseResult) =>
        {
            var handler = _host.Services.GetRequiredService<RootCommandHandler>();
            var task = handler.HandleAsync(parseResult);
            return task.GetAwaiter().GetResult();
        });

        // Interactive Command - async
        var interactiveHandler = _host.Services.GetRequiredService<InteractiveCommandHandler>();
        var interactiveCommand = new InteractiveCommand(interactiveHandler, "interactive", "Manage bookmarks interactively");
        interactiveCommand.AssignHandler();
        rootCommand.Add(interactiveCommand);

        // Link commands - async
        var linkHandler = _host.Services.GetRequiredService<LinkCommandHandler>();
        var linkCommand = new LinkCommand(linkHandler, "link", "Manage bookmarks links");
        linkCommand.AssignHandler();
        rootCommand.Add(linkCommand);

        // Add link command - async
        var addLinkHandler = _host.Services.GetRequiredService<LinkAddCommandHandler>();
        var addLinkCommand = new LinkAddCommand(addLinkHandler, "add", "Add a new bookmark link");
        addLinkCommand.AssignHandler();
        linkCommand.Add(addLinkCommand);

        // Remove link command - async
        var removeLinkHandler = _host.Services.GetRequiredService<LinkRemoveCommandHandler>();
        var removeLinkCommand = new LinkRemoveCommand(removeLinkHandler, "remove", "Removes a bookmark link");
        removeLinkCommand.AssignHandler();
        linkCommand.Add(removeLinkCommand);

        // Update link command - async
        var updateLinkHandler = _host.Services.GetRequiredService<LinkUpdateCommandHandler>();
        var updateLinkCommand = new LinkUpdateCommand(updateLinkHandler, "update", "Updates an existing bookmark");
        updateLinkCommand.AssignHandler();
        linkCommand.Add(updateLinkCommand);

        // Export command - async
        var exportHandler = _host.Services.GetRequiredService<ExportCommandHandler>();
        var exportCommand = new ExportCommand(exportHandler, "export", "Exports all bookmarks to a file.").AddOptions();
        exportCommand.AssignHandler();
        rootCommand.Add(exportCommand);

        // Import command - async
        var importHandler = _host.Services.GetRequiredService<ImportCommandHandler>();
        var importCommand = new ImportCommand(importHandler, "import", "Import bookmarks from a file").AddOptions();
        importCommand.AssignHandler();
        rootCommand.Add(importCommand);

        // Show command - async
        var showHandler = _host.Services.GetRequiredService<ShowCommandHandler>();
        var showCommand = new ShowCommand(showHandler, "show", "Show details of a specific bookmark");
        showCommand.AssignHandler();
        rootCommand.Add(showCommand);

        // Sync command - async
        var syncHandler = _host.Services.GetRequiredService<SyncCommandHandler>();
        var syncCommand = new SyncCommand(syncHandler, "sync", "Sync bookmarks with remote server")
            .AssignAction();
        rootCommand.Add(syncCommand);

        // Category command - async
        var categoryHandler = _host.Services.GetRequiredService<CategoryCommandHandler>();
        var categoryCommand = new CategoryCommand(categoryHandler, "category", "Manage bookmark categories")
            .AssignHandler();
        rootCommand.Add(categoryCommand);
    }

    private static void CreateLogger()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    /// <summary>
    /// Ensures that the database is created and applies any pending migrations asynchronously.
    /// </summary>
    /// <remarks>This method creates a new service scope to resolve the database context and applies 
    /// migrations to bring the database schema up to date. It should be called during application  startup to ensure
    /// the database is ready for use.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task EnsureDatabaseAsync()
    {
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookmarkrDbContext>();
        await context.Database.MigrateAsync();
    }

    private static void FreeSerilogLoggerOnShutdown()
    {
        // This even is raised when the process is about to exit
        // allowing you to perform cleanup tasks or save data
        AppDomain.CurrentDomain.ProcessExit += (s, e) => ExecuteShutdownTasks();
        // This even is triggered when the user pressed Ctrl+C or
        // Ctrl+Break. Doesn't cover all scenarios but useful for
        // user initiated terminations
        Console.CancelKeyPress += (s, e) => ExecuteShutdownTasks();
    }

    private static void ExecuteShutdownTasks()
    {
        Console.WriteLine("Performing shutdown tasks...");
        Log.CloseAndFlush();
    }

    private static void OnHandleRootCommand()
    {
        Console.WriteLine("Hello from the root command!");
    }
}
