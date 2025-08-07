using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace bookmarkr;

public class LinkUpdateCommandHandler : AsynchronousCommandLineAction
{
    private readonly BookMarkService _bookmarkService;

    public LinkUpdateCommandHandler(BookMarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var name = parseResult.GetValue<string>("name");
        var url = parseResult.GetValue<string>("url");
        if (name is not null
            && !string.IsNullOrWhiteSpace(name)
            && url is not null
            && !string.IsNullOrWhiteSpace(url))
        {
            OnHandleUpdateCommand(_bookmarkService, name, url);
        }

        return 0;
    }

    private static void OnHandleUpdateCommand(BookMarkService bookMarkService, string name, string url)
    {
        var bookmarks = bookMarkService.ExistingBookmarks;
        if (bookmarks is null || !bookmarks.Any())
        {
            CommandHelper.PrintConsoleMessage("Warning: no bookmarks currently present.", ConsoleColor.Yellow);

            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            CommandHelper.PrintConsoleMessage("Warning: bookmark does not exist. Use the `link add` command to add a new bookmark.",
            ConsoleColor.Yellow);

            return;
        }

        foundBookmark.Url = url;

        CommandHelper.PrintConsoleMessage("Bookmark updated successfully.", ConsoleColor.Green);
        CommandHelper.ListAll(bookMarkService);
    }
}
