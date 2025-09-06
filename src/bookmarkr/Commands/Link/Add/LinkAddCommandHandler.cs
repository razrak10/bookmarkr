using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.Service;
using System.CommandLine;

namespace bookmarkr;

public class LinkAddCommandHandler
{
    private readonly IBookmarkService _bookmarkService;

    public LinkAddCommandHandler(IBookmarkService service)
    {
        _bookmarkService = service;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var names = parseResult.GetValue<string[]>("name");
        var urls = parseResult.GetValue<string[]>("url");
        var categories = parseResult.GetValue<string[]>("category");

        if (names is null || urls is null || categories is null)
        {
            MessageHelper.ShowErrorMessage(["Provided bookmark name, urls or categories is null"]);
            return -1;
        }

        await OnHandleAddLinkCommandAsync(names, urls, categories);
        return 0;
    }

    private async Task OnHandleAddLinkCommandAsync(string[] names, string[] urls, string[] categories)
    {
        for (int i = 0; i < names.Length; i++)
        {
            var executionResult = await _bookmarkService.AddBookmarkAsync(names[i], urls[i], categories[i]);

            if (!executionResult.IsSuccess)
            {
                LogManager.LogError(executionResult.Message!, executionResult.Exception);
                MessageHelper.ShowErrorMessage(["Error occured while attempting to add bookmark", $"{executionResult.Message}"]);
                return;
            }

            MessageHelper.ShowSuccessMessage(["Bookmarks added successfully."]);
        }

        await MessageHelper.ListAll(_bookmarkService);
    }
}
