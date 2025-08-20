using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.Service;
using Spectre.Console;
using System.CommandLine;

namespace bookmarkr.Commands.Show;
public class ShowCommandHandler
{
    private readonly IBookmarkService _bookmarkService;

    public ShowCommandHandler(IBookmarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        string[]? bookmarkNames = parseResult.GetValue<string[]>("name");
        string? bookname = bookmarkNames?.FirstOrDefault();
        if (bookmarkNames is not null && !string.IsNullOrWhiteSpace(bookname))
        {
            await OnLinkShowCommandHandle(bookname);
            return 0;
        }

        return -1;
    }

    private async Task OnLinkShowCommandHandle(string bookmarkName)
    {
        var result = await _bookmarkService.GetBookmarkAsync(bookmarkName);

        if (!result.IsSuccess)
        {
            string message = $"Error while retrieving bookmark. Error: {result.Message}.";
            LogManager.LogError(message, result.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }

        Bookmark? foundBookmark = result.Value;

        Table table = new Table();
        table.AddColumn("[bold]Name[/]");
        table.AddColumn("[bold]URL[/]");
        table.AddColumn("[bold]Category[/]");

        table.AddRow(
            $"[yellow][bold]{Markup.Escape(foundBookmark!.Name)}[/][/]",
            $"[link]{Markup.Escape(foundBookmark.Url)}[/]",
            $"[green][italic]{Markup.Escape(foundBookmark.Category ?? "")}[/][/]"
        );

        AnsiConsole.Write(table);
    }
}