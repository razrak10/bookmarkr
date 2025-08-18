using System;
using System.CommandLine;
using bookmarkr.Helpers;
using bookmarkr.Logger;
using Spectre.Console;

namespace bookmarkr.Commands;

public class ChangeCommandHandler
{
    private readonly BookmarkService _bookmarkService;

    public ChangeCommandHandler(BookmarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public  async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var url = parseResult.GetValue<string>("forUrl");
        if (!string.IsNullOrWhiteSpace(url))
        {
            await OnChangeCommmandHandle(url);
            return 0;
        }
        return -1;
    }

    private async Task OnChangeCommmandHandle(string url)
    {
        bool isRunning = true;

        var result = await _bookmarkService.GetExistingCategoriesAsync();

        if (!result.IsSuccess)
        {
            LogManager.LogError($"Error occured while retrieving categories. Error: {result.Message}", result.Exception);
            MessageHelper.ShowErrorMessage([$"Error occured whule retrieving categories.", $"{result.Message}"]);
        }

        IEnumerable<string> categories = result.Value!;

        while (isRunning)
        {
            string selectedCategory = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[yellow]Select a category for the url:[/]")
                .AddChoices(categories)
            );

            var changeResult = await _bookmarkService.ChangeBookmarkCategoryAsync(url, selectedCategory);

            if (!result.IsSuccess)
            {
                LogManager.LogError($"Error occured while changing category. Error: {result.Message}", result.Exception);
                MessageHelper.ShowErrorMessage([$"Error occured while changing category.", $"{result.Message}"]);
                return;
            }

            if (changeResult.Value)
            {
                MessageHelper.ShowSuccessMessage([$"Bookmark category changed successfully. Selected category: '{selectedCategory}'"]);
                break;
            }
        }
    }
}
