using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using bookmarkr.Commands;
using Spectre.Console;

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
                services.AddSingleton<BookMarkService>();
            });
        });

        rootCommand.UseCommandHandler<RootCommandHandler>();

        // Interactive Command
        Command interactiveCommand = new Command("interactive", "Manage bookmarks interactively");
        interactiveCommand.UseCommandHandler<InteractiveCommandHandler>();
        rootCommand.Subcommands.Add(interactiveCommand);

        // List bookmarks
        var listOption = new Option<bool>("list", ["--list", "-l"]);
        var linkCommand = new Command("link", "Manage bookmarks links")
        {
            listOption
        };
        linkCommand.UseCommandHandler<LinkCommandHandler>();
        rootCommand.Subcommands.Add(linkCommand);

        // Add bookmarks
        Option nameOption = new Option<string[]>("name", ["--name", "-n"])
        {
            Required = true,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true
        };
        Option urlOption = new Option<string[]>("url", ["--url", "-u"])
        {
            Required = true,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true
        };
        urlOption.Validators.Add(result =>
        {
            foreach (Token token in result.Tokens)
            {
                if (string.IsNullOrWhiteSpace(token.Value))
                {
                    result.AddError("URL cannot be empty");
                    break;
                }
                else if (!Uri.TryCreate(token.Value, UriKind.Absolute, out _))
                {
                    result.AddError($"Invalid URL: {token.Value}");
                    break;
                }
            }
        });
        Option categoryOption = new Option<string[]>("category", ["--category", "-c"])
        {
            DefaultValueFactory = (_) => ["Read later"],
            Required = false,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true
        };
        categoryOption.CompletionSources.Add(_ =>
        {
            return ["Read later", "Tech books", "Cooking", "Social media"];
        });
        categoryOption.Validators.Add(result =>
        {
            var categories = result.GetValueOrDefault<string[]>();
            string[] allowedCategories = ["Read later", "Tech books", "Cooking", "Social media"];

            foreach (string category in categories)
            {
                if (!string.IsNullOrEmpty(category) && !allowedCategories.Contains(category))
                {
                    result.AddError($"Category must be one of: {string.Join(", ", allowedCategories)}");
                }
            }
        });

        var addLinkCommand = new Command("add", "Add a new bookmark link")
        {
            nameOption,
            urlOption,
            categoryOption
        };
        addLinkCommand.UseCommandHandler<LinkAddCommandHandler>();
        linkCommand.Subcommands.Add(addLinkCommand);

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
        Option<FileInfo> outputFileOption = new Option<FileInfo>("file", ["--file", "-f"])
        {
            Required = true,
            Description = "The output file that will store the bookmarks",
        };
        outputFileOption.AcceptLegalFileNamesOnly();
        Command exportCommand = new Command("export", "Exports all bookmarks to a file")
        {
            outputFileOption
        };
        exportCommand.UseCommandHandler<ExportCommandHandler>();
        rootCommand.Add(exportCommand);

        // Import bookmarks
        Option<FileInfo> inputFileOption = new Option<FileInfo>("file", ["--file", "-f"])
        {
            Required = true,
            Description = "The input file that contains the bookmarks to be imported",
        };
        inputFileOption.AcceptLegalFileNamesOnly();
        inputFileOption.AcceptExistingOnly();

        Option<bool> mergeFileOption = new Option<bool>("merge", ["--merge", "-m"])
        {
            Description = "Import file option to merge imported bookmarks with existing ones."
        };
        mergeFileOption.Arity = ArgumentArity.Zero;

        Command importCommand = new Command("import", "Exports all bookmarks to a file")
        {
            inputFileOption,
            mergeFileOption
        };
        importCommand.UseCommandHandler<ImportCommandHandler>();
        rootCommand.Add(importCommand);

        commandLineConfig.Parse(args).InvokeAsync();
    }

    //"https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#json-configuration-provider",

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
