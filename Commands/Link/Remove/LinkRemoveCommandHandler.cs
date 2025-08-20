using bookmarkr.ExecutionResult;
using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.Service;
using System.CommandLine;

namespace bookmarkr;

public class LinkRemoveCommandHandler
{
    private readonly IBookmarkService _bookmarkService;

    public LinkRemoveCommandHandler(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        string? name = parseResult.GetValue<string>("name");

        if (name is not null && !string.IsNullOrWhiteSpace(name))
        {
            await OnHandleRemoveCommand(name);
            return 0;
        }

        return -1;
    }

    private async Task OnHandleRemoveCommand(string name)
    {
        ConsoleColor prevColor = Console.ForegroundColor;

        ExecutionResult<IEnumerable<Bookmark>> result = await _bookmarkService.GetBookmarksAsync(false);

        if (!result.IsSuccess)
        {
            string message = $"Error occured while retrieving bookmarks. Error: {result.Message}";
            LogManager.LogError(message, result.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }

        List<Bookmark>? bookmarks = result.Value!.ToList();
        if (!bookmarks.Any())
        {
            string message = "No bookmarks currently present.";
            LogManager.LogInformation(message);
            MessageHelper.ShowWarningMessage([message]);
            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            MessageHelper.ShowWarningMessage(["Bookmark does not exist."]);
            return;
        }

        var removeResult = await _bookmarkService.RemoveLinkAsync(foundBookmark.Name);

        if (!removeResult.IsSuccess)
        {
            LogManager.LogError(removeResult.Message!, removeResult.Exception);
            MessageHelper.ShowErrorMessage(["Error occured while attempting to add bookmark", $"{removeResult.Message}"]);
            return;
        }

        MessageHelper.ShowSuccessMessage(["Bookmark removed successfully."]);
        await MessageHelper.ListAll(_bookmarkService);
    }
}
