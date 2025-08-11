using bookmarkr.Commands;
using bookmarkr.Commands.Category;
using bookmarkr.Commands.Export;
using bookmarkr.Commands.Import;
using bookmarkr.Commands.Interactive;
using bookmarkr.Commands.Sync;
using bookmarkr.Options;
using bookmarkr.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.CommandLine;
using System.CommandLine.Hosting;

namespace bookmarkr;

class Program
{
    static void Main(string[] args)
    {
        FreeSerilogLoggerOnShutdown();

        CreateLogger();

        RootCommand rootCommand = new RootCommand("Bookmarkr is a bookmark manager provided as a CLI application");

        CommandLineConfiguration commandLineConfig = new CommandLineConfiguration(rootCommand);
        commandLineConfig.UseHost((builder) =>
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IBookMarkService, BookMarkService>();
                services.AddHttpClient();
                services.AddHttpClient("bookmarkrSyncr", client =>
                {
                    client.BaseAddress = new Uri("https://bookmarkrsyncr-api.azurewebsites.net");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "Bookmarkr");
                });

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
            });
        });

        rootCommand.UseCommandHandler<RootCommandHandler>();

        // Interactive Command
        Command interactiveCommand = new InteractiveCommand("interactive", "Manage bookmarks interactively")
        .AssignCommandHandler();
        rootCommand.Subcommands.Add(interactiveCommand);

        // List bookmarks
        var listOption = new ListOption("list", ["--list", "-l"]);
        var linkCommand = new Command("link", "Manage bookmarks links")
        {
            listOption
        };
        linkCommand.UseCommandHandler<LinkCommandHandler>();
        rootCommand.Subcommands.Add(linkCommand);

        // Add bookmarks
        Option nameOption = new NameOption("name", ["--name", "-n"], arity: ArgumentArity.OneOrMore);
        Option urlOption = new UrlOption("url", ["--url", "-u"], arity: ArgumentArity.OneOrMore).AddDefaultValidators();
        Option categoryOption = new CategoryOption("category", ["--category", "-c"], arity: ArgumentArity.OneOrMore, defaultValues: ["Read later"])
        .AddDefaultValidators(["Read later", "Tech books", "Cooking", "Social media"])
        .AddDefaultCompletionSources(["Read later", "Tech books", "Cooking", "Social media"]);
        var addLinkCommand = new Command("add", "Add a new bookmark link")
        {
            nameOption,
            urlOption,
            categoryOption
        };
        addLinkCommand.UseCommandHandler<LinkAddCommandHandler>();
        linkCommand.Subcommands.Add(addLinkCommand);

        //TODO: Extract the rest of the options into their own classes
        // Remove bookmarks
        var removeOption = new Option<string>("name", ["--name", "-n"]);
        var removeLinkCommand = new Command("remove", "Removes a bookmark link")
        {
            removeOption
        };
        removeLinkCommand.UseCommandHandler<LinkRemoveCommandHandler>();
        linkCommand.Subcommands.Add(removeLinkCommand);

        // Update bookmarks
        Command updateLinkCommand = new Command("update", "Updates and existing bookmark.")
        {
            nameOption,
            urlOption
        };
        updateLinkCommand.UseCommandHandler<LinkUpdateCommandHandler>();
        linkCommand.Subcommands.Add(updateLinkCommand);

        // Export bookmarks
        Command exportCommand = new ExportCommand("export", "Exports all bookmarks to a file.")
        .AddOptions()
        .AssignCommandHandler();
        rootCommand.Add(exportCommand);

        // Import bookmarks
        Command importCommand = new ImportCommand("import", "Imports all bookmarks from a file.")
        .AddOptions()
        .AssignCommandHandler();
        rootCommand.Add(importCommand);

        // Show bookmark
        Command showCommand = new Command("show", "Shows bookmark in a formatted table.")
        {
            nameOption
        };
        showCommand.UseCommandHandler<ShowCommandHandler>();
        linkCommand.Subcommands.Add(showCommand);

        // Change bookmark category
        Command categoryCommand = new Command("category", "Bookmark category specific functions.");
        categoryCommand.UseCommandHandler<CategoryCommandHandler>();

        Option forUrlOption = new Option<string>("forUrl", ["--for-url", "-fu"])
        {
            Required = true,
            Arity = ArgumentArity.ExactlyOne
        };
        Command changeCommand = new Command("change", "Change bookmark category.")
        {
            forUrlOption
        };
        changeCommand.UseCommandHandler<ChangeCommandHandler>();

        rootCommand.Add(categoryCommand);
        categoryCommand.Subcommands.Add(changeCommand);

        // Sync 
        SyncCommand syncCommand = new SyncCommand("sync", "Sync bookmarks with external bookmarks storage.")
            .AssignCommandHandler();
        rootCommand.Add(syncCommand);

        commandLineConfig.Parse(args).InvokeAsync();
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
