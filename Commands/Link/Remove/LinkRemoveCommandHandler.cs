using System.CommandLine;

namespace bookmarkr;

public class LinkRemoveCommandHandler
{
    private readonly BookMarkService _bookmarkService;

    public LinkRemoveCommandHandler(BookMarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var name = parseResult.GetValue<string>("name");

        if (name is not null && !string.IsNullOrWhiteSpace(name))
        {
            OnHandleRemoveCommand(_bookmarkService, name);
            return Task.FromResult(0);
        }

        return Task.FromResult(-1);
    }

    private static void OnHandleRemoveCommand(BookMarkService bookMarkService, string name)
    {
        var prevColor = Console.ForegroundColor;

        var bookmarks = bookMarkService.ExistingBookmarks;
        if (bookmarks is null || !bookmarks.Any())
        {
            CommandHelper.ShowWarningMessage(["Warning: no bookmarks currently present."]);

            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            CommandHelper.ShowWarningMessage(["Bookmark does not exist."]);

            return;
        }

        bookmarks.Remove(foundBookmark);

        CommandHelper.ShowSuccessMessage(["Bookmark removed successfully."]);
        CommandHelper.ListAll(bookMarkService);
    }
}
