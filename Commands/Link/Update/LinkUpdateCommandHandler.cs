using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.Service;
using System.CommandLine;

namespace bookmarkr;

public class LinkUpdateCommandHandler
{
    private readonly IBookmarkService _bookmarkService;

    public LinkUpdateCommandHandler(IBookmarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        string[]? names = parseResult.GetValue<string[]>("name");
        string[]? urls = parseResult.GetValue<string[]>("url");

        if (names is not null
            && names.Any()
            && urls is not null
            && urls.Any())
        {
            await OnHandleUpdateCommand(names, urls);
        }

        return 0;
    }

    private async Task OnHandleUpdateCommand(string[] names, string[] urls)
    {
        var result = await _bookmarkService.GetBookmarksAsync(true);

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

        for (int i = 0; i < names.Count(); i++)
        {
            var foundBookmark = bookmarks.Find(b => string.Equals(b.Name, names[i], StringComparison.OrdinalIgnoreCase));

            if (foundBookmark is null)
            {
                MessageHelper.ShowWarningMessage(["Bookmark does not exist. Use the `link add` command to add a new bookmark."]);
                return;
            }

            foundBookmark.Url = urls[i];

            await _bookmarkService.SaveChangesAsync();

            MessageHelper.ShowSuccessMessage(["Bookmark(s) updated successfully."]);
        }

        await MessageHelper.ListAll(_bookmarkService);
    }
}
