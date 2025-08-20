using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.Service;
using System.CommandLine;

namespace bookmarkr;

public class LinkCommandHandler
{
    private readonly IBookmarkService _bookmarkService;

    public LinkCommandHandler(IBookmarkService service)
    {
        _bookmarkService = service;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        await OnHandleLinkCommand();
        return 0;
    }

    private async Task OnHandleLinkCommand()
    {
        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> executionResult = await _bookmarkService.GetBookmarksAsync(isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            string message = $"Error occured when retrieving bookmarks. Error: {executionResult.Message}";
            LogManager.LogError(message, executionResult.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }

        IEnumerable<Bookmark> bookmarks = executionResult.Value!;
        if (!bookmarks.Any())
        {
            string message = "No bookmarks currently present.";
            LogManager.LogInformation(message);
            MessageHelper.ShowWarningMessage([message]);
            return;
        }

        Bookmark[] bookmarksArray = bookmarks.ToArray();

        for (int i = 0; i < bookmarks.Count(); i++)
        {
            Console.WriteLine($"# <name {i + 1}>");
            Console.WriteLine($"<{bookmarksArray[i].Url}>\n");
        }
    }
}
