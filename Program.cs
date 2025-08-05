using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace bookmarkr;

class Program
{
    private static readonly BookMarkService _service = new BookMarkService();

    static int Main(string[] args)
    {
        RootCommand rootCommand = new RootCommand("Bookmarkr is a bookmark manager provided as a CLI application");

        rootCommand.SetAction(parseResult =>
        {
            OnHandleRootCommand();
        });

        // List bookmarks
        var listOption = new Option<bool>("list", ["--list", "-l"]);
        var linkCommand = new Command("link", "Manage bookmarks links")
        {
            listOption
        };
        rootCommand.Subcommands.Add(linkCommand);
        linkCommand.SetAction(parseResult =>
        {
            OnHandleLinkCommand();
        });

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
        linkCommand.Subcommands.Add(addLinkCommand);
        addLinkCommand.SetAction(parseResult =>
        {
            var names = parseResult.GetValue<string[]>("name");
            var urls = parseResult.GetValue<string[]>("url");
            var categories = parseResult.GetValue<string[]>("category");

            if (names is null || urls is null || categories is null)
            {
                PrintConsoleMessage("Provided bookmark name, urls or categories is null", ConsoleColor.Red);
                return;
            }

            OnHandleAddLinkCommand(names, urls, categories);
        });

        // Remove bookmarks
        var removeOption = new Option<string>("name", ["--name", "-n"]);
        var removeLinkCommand = new Command("remove", "Removes a bookmark link")
        {
            removeOption
        };
        linkCommand.Subcommands.Add(removeLinkCommand);
        removeLinkCommand.SetAction(parseResult =>
        {
            var name = parseResult.GetValue<string>("name");

            if (name is not null && !string.IsNullOrWhiteSpace(name))
            {
                OnHandleRemoveCommand(name);
            }

            return -1;
        });

        // Update bookmarks
        Command updateLinkCommand = new Command("update", "Updates and existing bookmark.")
        {
            nameOption,
            urlOption
        };
        linkCommand.Subcommands.Add(updateLinkCommand);
        updateLinkCommand.SetAction(parseResult =>
        {
            var name = parseResult.GetValue<string>("name");
            var url = parseResult.GetValue<string>("url");
            if (name is not null
                && !string.IsNullOrWhiteSpace(name)
                && url is not null
                && !string.IsNullOrWhiteSpace(url))
            {
                OnHandleUpdateCommand(name, url);
            }
        });

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
        rootCommand.Add(exportCommand);
        exportCommand.SetAction(parseResult =>
        {
            FileInfo? outputFile = parseResult.GetValue(outputFileOption);

            if (outputFile is not null)
            {
                OnExportCommand(outputFile);
            }
        });

        // Import bookmarks
        Option<FileInfo> inputFileOption = new Option<FileInfo>("file", ["--file", "-f"])
        {
            Required = true,
            Description = "The input file that contains the bookmarks to be imported",
        };
        inputFileOption.AcceptLegalFileNamesOnly();
        inputFileOption.AcceptExistingOnly();
        //TODO: Add merge option

        Command importCommand = new Command("import", "Exports all bookmarks to a file")
        {
            inputFileOption
        };
        rootCommand.Add(importCommand);
        importCommand.SetAction(parseResult =>
        {
            FileInfo? inputFile = parseResult.GetValue(inputFileOption);

            if (inputFile is not null)
            {
                OnImportCommand(inputFile);
            }
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static void OnImportCommand(FileInfo inputFile)
    {
        List<Bookmark> bookmarks = new List<Bookmark>();
        string json;
        try
        {
            json = File.ReadAllText(inputFile.FullName);

        }
        catch (Exception ex)
        {
            PrintConsoleMessage($"Error accessing file: {ex.Message}");
            return;
        }
        try
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                bookmarks = JsonSerializer.Deserialize<List<Bookmark>>(json) ?? new List<Bookmark>();
            }
        }
        catch (JsonException ex)
        {
            PrintConsoleMessage($"Error occured while attempting to deserialize the imports file, exception:{ex.Message}", ConsoleColor.Red);
            return;
        }

        _service.ImportBookmarks(bookmarks);
        PrintConsoleMessage($"Successfully imported {bookmarks.Count} bookmarks!", ConsoleColor.Green);
    }

    private static void PrintConsoleMessage(string v)
    {
        throw new NotImplementedException();
    }

    private static void OnExportCommand(FileInfo outputFile)
    {
        var bookmarks = _service.GetAll();
        string json = JsonSerializer.Serialize(bookmarks,
        new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputFile.FullName, json);
    }

    private static void OnHandleRootCommand()
    {
        Console.WriteLine("Hello from the root command!");
    }

    private static void OnHandleLinkCommand()
    {
        var bookmarks = _service.Bookmarks;

        if (bookmarks is null || !bookmarks.Any())
        {
            PrintConsoleMessage("Warning: no bookmarks currently present", ConsoleColor.Yellow);

            return;
        }

        for (int i = 0; i < bookmarks.Count; i++)
        {
            Console.WriteLine($"# <name {i + 1}>");
            Console.WriteLine($"<{_service.Bookmarks[i].Url}>\n");
        }
    }

    private static void OnHandleRemoveCommand(string name)
    {
        var prevColor = Console.ForegroundColor;

        var bookmarks = _service.Bookmarks;
        if (bookmarks is null || !bookmarks.Any())
        {
            PrintConsoleMessage("Warning: no bookmarks currently present.", ConsoleColor.Yellow);

            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            PrintConsoleMessage("Warning: bookmark does not exist.", ConsoleColor.Yellow);

            return;
        }

        bookmarks.Remove(foundBookmark);

        PrintConsoleMessage("Bookmark removed successfully.", ConsoleColor.Green);
    }

    private static void OnHandleUpdateCommand(string name, string url)
    {
        var bookmarks = _service.Bookmarks;
        if (bookmarks is null || !bookmarks.Any())
        {
            PrintConsoleMessage("Warning: no bookmarks currently present.", ConsoleColor.Yellow);

            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            PrintConsoleMessage("Warning: bookmark does not exist. Use the `link add` command to add a new bookmark.",
            ConsoleColor.Yellow);

            return;
        }

        foundBookmark.Url = url;

        PrintConsoleMessage("Bookmark updated successfully.", ConsoleColor.Green);
    }

    private static void OnHandleAddLinkCommand(string[] names, string[] urls, string[] categories)
    {
        for (int i = 0; i < names.Length; i++)
        {
            _service.AddLink(names[i], urls[i], categories[i]);
            PrintConsoleMessage("Bookmark updated successfully.", ConsoleColor.Green);
        }
        ListAll();
    }

    private static void PrintConsoleMessage(string message, ConsoleColor color)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"{message}");
        Console.ForegroundColor = prevColor;
    }

    private static void ListAll()
    {
        foreach (Bookmark bookmark in _service.Bookmarks)
        {
            Console.WriteLine($"Name: '{bookmark.Name}' | URL: '{bookmark.Url}' | Category: '{bookmark.Category}'");
        }
    }
}
