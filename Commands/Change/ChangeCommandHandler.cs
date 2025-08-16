using System;
using System.CommandLine;
using Spectre.Console;

namespace bookmarkr.Commands;

public class ChangeCommandHandler
{
    private readonly BookMarkService _bookmarkService;

    public ChangeCommandHandler(BookMarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var url = parseResult.GetValue<string>("forUrl");
        if (!string.IsNullOrWhiteSpace(url))
        {
            OnChangeCommmandHandle(url);
            return Task.FromResult(0);
        }
        return Task.FromResult(-1);
    }

    private void OnChangeCommmandHandle(string url)
    {
        bool isRunning = true;
        IEnumerable<string> categories = _bookmarkService.GetCategories();

        while (isRunning)
        {
            string selectedCategory = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[yellow]Select a category for the url:[/]")
                .AddChoices(categories)
            );

            bool success = _bookmarkService.ChangeBookmarkCategoryAsync(url, selectedCategory);

            if (success)
            {
                AnsiConsole.MarkupLine($"[green]Bookmark category changed successfully. Selected category: '{selectedCategory}'[/]");
                break;
            }
        }
    }
}
