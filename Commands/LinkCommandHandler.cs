using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace bookmarkr;

public class LinkCommandHandler : AsynchronousCommandLineAction
{
    private readonly BookMarkService _bookmarkService;

    public LinkCommandHandler(BookMarkService service)
    {
        _bookmarkService = service;
    }

    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        OnHandleLinkCommand(_bookmarkService);
        return Task.FromResult(0);
    }

    private static void OnHandleLinkCommand(BookMarkService bookMarkService)
    {
        var bookmarks = bookMarkService.ExistingBookmarks;

        if (bookmarks is null || !bookmarks.Any())
        {
            PrintConsoleMessage("Warning: no bookmarks currently present", ConsoleColor.Yellow);

            return;
        }

        for (int i = 0; i < bookmarks.Count; i++)
        {
            Console.WriteLine($"# <name {i + 1}>");
            Console.WriteLine($"<{bookMarkService.ExistingBookmarks[i].Url}>\n");
        }
    }

    private static void PrintConsoleMessage(string message, ConsoleColor color = ConsoleColor.White)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"{message}");
        Console.ForegroundColor = prevColor;
    }
}
