using bookmarkr.ExecutionResult;
using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.ServiceAgent;
using System.CommandLine;

namespace bookmarkr.Commands.Sync;

public class SyncCommandHandler
{
    private readonly BookmarkService _bookmarkService;
    private readonly IBookmarkrSyncrServiceAgent _serviceAgent;

    public SyncCommandHandler(BookmarkService bookMarkService, IBookmarkrSyncrServiceAgent serviceAgent)
    {
        _bookmarkService = bookMarkService;
        _serviceAgent = serviceAgent;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellation = default)
    {
        await OnSyncCommand();
        return 0;
    }

    private async Task OnSyncCommand()
    {
        var result = await _bookmarkService.GetBookmarksAsync(false);

        if (!result.IsSuccess)
        {
            string message = $"Error occured while retrieving retrievedBookmarks: Error: {result.Message}";
            LogManager.LogError(message, result.Exception);
            MessageHelper.ShowErrorMessage([message]);
        }

        var retrievedBookmarks = result.Value!.ToList();

        if (!retrievedBookmarks.Any())
        {
            MessageHelper.ShowWarningMessage(["No retrievedBookmarks currently present."]);
            return;
        }

        ExecutionResult<List<Bookmark>> executionResult = await _serviceAgent.SyncBookmarksAsync(retrievedBookmarks);

        if (!executionResult.IsSuccess)
        {
            string message = $"Error occured while syncing bookmarks. Error:{executionResult.Message}";
            LogManager.LogError(message, executionResult.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }
        else
        {

            MessageHelper.ShowSuccessMessage([$"Bookmarks synchronized successfully."]);
        }
    }
}