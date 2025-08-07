using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace bookmarkr;

public class LinkRemoveCommandHandler : AsynchronousCommandLineAction
{
    private readonly BookMarkService _bookmarkService;

    public LinkRemoveCommandHandler(BookMarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var name = parseResult.GetValue<string>("name");

        if (name is not null && !string.IsNullOrWhiteSpace(name))
        {
            OnHandleRemoveCommand(_bookmarkService, name);
        }

        return -1;
    }

    private static void OnHandleRemoveCommand(BookMarkService bookMarkService, string name)
    {
        var prevColor = Console.ForegroundColor;

        var bookmarks = bookMarkService.ExistingBookmarks;
        if (bookmarks is null || !bookmarks.Any())
        {
            CommandHelper.PrintConsoleMessage("Warning: no bookmarks currently present.", ConsoleColor.Yellow);

            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            CommandHelper.PrintConsoleMessage("Warning: bookmark does not exist.", ConsoleColor.Yellow);

            return;
        }

        bookmarks.Remove(foundBookmark);

        CommandHelper.PrintConsoleMessage("Bookmark removed successfully.", ConsoleColor.Green);
        CommandHelper.ListAll(bookMarkService);
    }
}
