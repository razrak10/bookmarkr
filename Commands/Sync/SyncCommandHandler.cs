using bookmarkr.ExecutionResult;
using bookmarkr.Helpers;
using bookmarkr.ServiceAgent;
using System.CommandLine;

namespace bookmarkr.Commands.Sync;

public class SyncCommandHandler
{
    private readonly BookMarkService _bookmarkService;
    private readonly IBookmarkrSyncrServiceAgent _serviceAgent;

    public SyncCommandHandler(BookMarkService bookMarkService, IBookmarkrSyncrServiceAgent serviceAgent)
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
        List<Bookmark> retrievedBookmarks = _bookmarkService.GetAll().ToList();

        ExecutionResult<List<Bookmark>> executionResult = await _serviceAgent.SyncBookmarksAsync(retrievedBookmarks);

        if (!executionResult.IsSuccess)
        {
            CommandHelper.ShowErrorMessage([$"{executionResult.Message}", $"{executionResult.Exception?.Message}"]);
        }
        else
        {

            CommandHelper.ShowSuccessMessage([$"Bookmarks synchronized successfully."]);
        }
    }
}