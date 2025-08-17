using Spectre.Console;

namespace bookmarkr.Helpers;

public class MessageHelper
{
    public static async Task ListAll(BookmarkService bookmarkService)
    {
        var result = await bookmarkService.GetBookmarksAsync(false);

        if (result.IsSuccess && result.Value.Any())
        {
            foreach (Bookmark bookmark in result.Value)
            {
                Console.WriteLine($"Name: '{bookmark.Name}' | URL: '{bookmark.Url}' | Category: '{bookmark.Category}'");
            }
        }
    }

    public static void ShowErrorMessage(string[] messages)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        AnsiConsole.MarkupLine(
            Emoji.Known.CrossMark + " [bold red]ERROR[/] :cross_mark:"
        );
        foreach (string message in messages)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]{message}[/]");
        }
    }

    public static void ShowWarningMessage(string[] messages)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var m = new Markup(Emoji.Known.Warning + "[bold yellow]Warning[/] :warning:");
        m.Centered();
        AnsiConsole.Write(m);
        AnsiConsole.WriteLine();
        foreach (string message in messages)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow]{message}[/]");
        }
    }

    public static void ShowSuccessMessage(string[] messages)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        AnsiConsole.MarkupLine(Emoji.Known.BeatingHeart + " [bold green]SUCCESS[/] :beating_heart:");
        foreach (string message in messages)
        {
            AnsiConsole.MarkupLineInterpolated($"[green]{message}[/]");
        }
    }
}
