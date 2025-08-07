using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using bookmarkr.Models;
using Serilog;

namespace bookmarkr;

public class ImportCommandHandler : AsynchronousCommandLineAction
{
    private readonly BookMarkService _bookmarkService;

    public ImportCommandHandler(BookMarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {

        FileInfo? inputFile = parseResult.GetValue<FileInfo>("file");
        bool merge = parseResult.GetValue<bool>("merge");

        if (inputFile is not null)
        {
            OnImportCommand(inputFile, merge);
        }

        return -1;
    }

    private void OnImportCommand(FileInfo inputFile, bool merge = false)
    {
        List<Bookmark> bookmarks = new List<Bookmark>();
        string json;
        try
        {
            json = File.ReadAllText(inputFile.FullName);

        }
        catch (Exception ex)
        {
            CommandHelper.PrintConsoleMessage($"Error accessing file: {ex.Message}", ConsoleColor.Red);
            return;
        }
        try
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                bookmarks = JsonSerializer.Deserialize<List<Bookmark>>(json) ?? new List<Bookmark>();
            }
        }
        catch (JsonException ex)
        {
            CommandHelper.PrintConsoleMessage($"Error occured while attempting to deserialize the imports file, exception:{ex.Message}", ConsoleColor.Red);
            return;
        }

        foreach (Bookmark bookmark in bookmarks)
        {
            BookMarkConflictModel? conflictBookmark = _bookmarkService.Import(bookmark, merge);
            if (conflictBookmark is not null)
            {
                Log.Information($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Bookmark updated | name changed from '{conflictBookmark.OriginalName}' to '{conflictBookmark.UpdatedName}' for URL '{conflictBookmark.Url}'");
            }
        }
    }
}
