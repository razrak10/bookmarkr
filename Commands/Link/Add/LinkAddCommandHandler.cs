using bookmarkr.Helpers;
using bookmarkr.Logger;
using System.CommandLine;

namespace bookmarkr;

public class LinkAddCommandHandler
{
    private readonly BookMarkService _bookmarkService;

    public LinkAddCommandHandler(BookMarkService service)
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
            CommandHelper.ShowErrorMessage(["Provided bookmark name, urls or categories is null"]);
            return Task.FromResult(-1);
        }

        OnHandleAddLinkCommandAsync(_bookmarkService, names, urls, categories);
        return Task.FromResult(0);
    }

    private async static void OnHandleAddLinkCommandAsync(
        BookMarkService bookmarkService, string[] names, string[] urls, string[] categories)
    {
        for (int i = 0; i < names.Length; i++)
        {
            var executionResult = await bookmarkService.AddLinkAsync(names[i], urls[i], categories[i]);

            if (!executionResult.IsSuccess)
            {
                LogManager.LogError(executionResult.Message, executionResult.Exception);
                CommandHelper.ShowErrorMessage(["Error occured while attempting to add bookmark", $"{executionResult.Message}"]);
                return;
            }

            CommandHelper.ShowSuccessMessage(["Bookmark updated successfully."]);
        }

        CommandHelper.ListAll(bookmarkService);
    }
}
