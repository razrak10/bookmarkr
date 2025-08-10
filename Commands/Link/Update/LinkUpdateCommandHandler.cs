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

    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
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

        return Task.FromResult(0);
    }

    private static void OnHandleUpdateCommand(BookMarkService bookMarkService, string name, string url)
    {
        var bookmarks = bookMarkService.ExistingBookmarks;
        if (bookmarks is null || !bookmarks.Any())
        {
            CommandHelper.ShowWarningMessage(["No bookmarks currently present."]);

            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            CommandHelper.ShowWarningMessage(["Bookmark does not exist. Use the `link add` command to add a new bookmark."]);

            return;
        }

        foundBookmark.Url = url;

        CommandHelper.ShowSuccessMessage(["Bookmark updated successfully."]);
        CommandHelper.ListAll(bookMarkService);
    }
}
