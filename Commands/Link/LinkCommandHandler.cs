using bookmarkr.Helpers;
using bookmarkr.Logger;
using System.CommandLine;

namespace bookmarkr;

public class LinkCommandHandler
{
    private readonly BookMarkService _bookmarkService;

    public LinkCommandHandler(BookMarkService service)
    {
        _bookmarkService = service;
    }

    public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        OnHandleLinkCommand(_bookmarkService);
        return Task.FromResult(0);
    }

    private async static void OnHandleLinkCommand(BookMarkService bookMarkService)
    {
        var executionResult = await bookMarkService.GetBookmarksAsync(isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            LogManager.LogError(executionResult.Message, executionResult.Exception);
            CommandHelper.ShowErrorMessage([$"No bookmarks currently present", $"{executionResult.Message}"]);
            return;
        }

        Bookmark[] bookmarks = executionResult.Value.ToArray();

        for (int i = 0; i < bookmarks.Count(); i++)
        {
            Console.WriteLine($"# <name {i + 1}>");
            Console.WriteLine($"<{bookmarks[i].Url}>\n");
        }
    }
}
