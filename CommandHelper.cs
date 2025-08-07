using System;

namespace bookmarkr;

public class CommandHelper
{
    public static void ListAll(BookMarkService bookMarkService)
    {
        foreach (Bookmark bookmark in bookMarkService.ExistingBookmarks)
        {
            Console.WriteLine($"Name: '{bookmark.Name}' | URL: '{bookmark.Url}' | Category: '{bookmark.Category}'");
        }
    }

    public static void PrintConsoleMessage(string message, ConsoleColor color = ConsoleColor.White)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"{message}");
        Console.ForegroundColor = prevColor;
    }
}
