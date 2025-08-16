using Spectre.Console;
using System.CommandLine;

namespace bookmarkr.Commands.Show;
public class ShowCommandHandler
{
    private readonly BookMarkService _bookmarkService;

    public ShowCommandHandler(BookMarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        string[]? bookmarkNames = parseResult.GetValue<string[]>("name");
        string? bookname = bookmarkNames?.FirstOrDefault();
        if (bookmarkNames is not null && !string.IsNullOrWhiteSpace(bookname))
        {
            OnLinkShowCommandHandle(bookname);
            return Task.FromResult(0);
        }

        return Task.FromResult(-1);
    }

    private void OnLinkShowCommandHandle(string bookmarkName)
    {
        Bookmark? foundBookmark = _bookmarkService.GetBookmarkAsync(bookmarkName);

        Table table = new Table();
        table.AddColumn("[bold]Name[/]");
        table.AddColumn("[bold]URL[/]");
        table.AddColumn("[bold]Category[/]");

        if (foundBookmark is not null)
        {
            table.AddRow(
                $"[yellow][bold]{Markup.Escape(foundBookmark.Name)}[/][/]",
                $"[link]{Markup.Escape(foundBookmark.Url)}[/]",
                $"[green][italic]{Markup.Escape(foundBookmark.Category)}[/][/]"
            );

            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No bookmark found.[/]");
        }
    }
}
