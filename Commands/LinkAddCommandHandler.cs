using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace bookmarkr;

public class LinkAddCommandHandler : AsynchronousCommandLineAction
{
    private readonly BookMarkService _bookmarkService;

    public LinkAddCommandHandler(BookMarkService service)
    {
        _bookmarkService = service;
    }

    public async override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var names = parseResult.GetValue<string[]>("name");
        var urls = parseResult.GetValue<string[]>("url");
        var categories = parseResult.GetValue<string[]>("category");

        if (names is null || urls is null || categories is null)
        {
            CommandHelper.PrintConsoleMessage("Provided bookmark name, urls or categories is null", ConsoleColor.Red);
            return -1;
        }

        OnHandleAddLinkCommand(_bookmarkService, names, urls, categories);
        return 0;
    }

    private static void OnHandleAddLinkCommand(
        BookMarkService bookmarkService, string[] names, string[] urls, string[] categories)
    {
        for (int i = 0; i < names.Length; i++)
        {
            bookmarkService.AddLink(names[i], urls[i], categories[i]);
            CommandHelper.PrintConsoleMessage("Bookmark updated successfully.", ConsoleColor.Green);
        }

        CommandHelper.ListAll(bookmarkService);
    }
}
