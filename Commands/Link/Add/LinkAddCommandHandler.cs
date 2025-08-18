using bookmarkr.Helpers;
using bookmarkr.Logger;
using System.CommandLine;

namespace bookmarkr;

public class LinkAddCommandHandler
{
    private readonly BookmarkService _bookmarkService;

    public LinkAddCommandHandler(BookmarkService service)
    {
        _bookmarkService = service;
    }

    public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var names = parseResult.GetValue<string[]>("name");
        var urls = parseResult.GetValue<string[]>("url");
        var categories = parseResult.GetValue<string[]>("category");

        if (names is null || urls is null || categories is null)
        {
            MessageHelper.ShowErrorMessage(["Provided bookmark name, urls or categories is null"]);
            return Task.FromResult(-1);
        }

        OnHandleAddLinkCommandAsync(_bookmarkService, names, urls, categories);
        return Task.FromResult(0);
    }

    private async static void OnHandleAddLinkCommandAsync(
        BookmarkService bookmarkService, string[] names, string[] urls, string[] categories)
    {
        for (int i = 0; i < names.Length; i++)
        {
            var executionResult = await bookmarkService.AddLinkAsync(names[i], urls[i], categories[i]);

            if (!executionResult.IsSuccess)
            {
                LogManager.LogError(executionResult.Message!, executionResult.Exception);
                MessageHelper.ShowErrorMessage(["Error occured while attempting to add bookmark", $"{executionResult.Message}"]);
                return;
            }

            MessageHelper.ShowSuccessMessage(["Bookmarks added successfully."]);
        }

        await MessageHelper.ListAll(bookmarkService);
    }
}
