using bookmarkr.Helpers;
using bookmarkr.Logger;
using System.CommandLine;

namespace bookmarkr;

public class LinkUpdateCommandHandler
{
    private readonly BookmarkService _bookmarkService;

    public LinkUpdateCommandHandler(BookmarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var name = parseResult.GetValue<string>("name");
        var url = parseResult.GetValue<string>("url");

        if (name is not null
            && !string.IsNullOrWhiteSpace(name)
            && url is not null
            && !string.IsNullOrWhiteSpace(url))
        {
            await OnHandleUpdateCommand(_bookmarkService, name, url);
        }

        return 0;
    }

    private async Task OnHandleUpdateCommand(BookmarkService bookMarkService, string name, string url)
    {
        var result = await _bookmarkService.GetBookmarksAsync(false);

        if (!result.IsSuccess)
        {
            string message = $"Error occured while retrieving bookmarks: {result.Message}.";
            LogManager.LogError(message, result.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }

        List<Bookmark> bookmarks = result.Value!.ToList();

        if (!bookmarks.Any())
        {
            MessageHelper.ShowWarningMessage(["No bookmarks currently present."]);
            return;
        }

        var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        if (foundBookmark is null)
        {
            MessageHelper.ShowWarningMessage(["Bookmark does not exist. Use the `link add` command to add a new bookmark."]);
            return;
        }

        foundBookmark.Url = url;

        MessageHelper.ShowSuccessMessage(["Bookmark updated successfully."]);
        await MessageHelper.ListAll(bookMarkService);
    }
}
