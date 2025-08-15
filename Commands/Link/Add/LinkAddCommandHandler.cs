using bookmarkr.Helpers;
using System;
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

        OnHandleAddLinkCommand(_bookmarkService, names, urls, categories);
        return Task.FromResult(0);
    }

    private static void OnHandleAddLinkCommand(
        BookMarkService bookmarkService, string[] names, string[] urls, string[] categories)
    {
        for (int i = 0; i < names.Length; i++)
        {
            bookmarkService.AddLink(names[i], urls[i], categories[i]);
            CommandHelper.ShowErrorMessage(["Bookmark updated successfully."]);
        }

        CommandHelper.ListAll(bookmarkService);
    }
}
