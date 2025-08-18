using System;
using System.CommandLine;
using System.Text.Json;
using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.Models;
using Serilog;

namespace bookmarkr;

public class ImportCommandHandler
{
    private readonly BookmarkService _bookmarkService;

    public ImportCommandHandler(BookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        FileInfo? inputFile = parseResult.GetValue<FileInfo>("file");
        bool merge = parseResult.GetValue<bool>("merge");

        if (inputFile is not null)
        {
            await OnImportCommand(inputFile, merge);
            return 0;
        }

        return -1;
    }

    private async Task OnImportCommand(FileInfo inputFile, bool merge = false)
    {
        List<Bookmark> bookmarks = new List<Bookmark>();
        string json;

        try
        {
            json = File.ReadAllText(inputFile.FullName);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogManager.LogInformation($"Insufficient permission to access the file {inputFile.FullName}", ex);
            return;
        }
        catch (DirectoryNotFoundException ex)
        {
            LogManager.LogInformation($"The file {inputFile.FullName} cannot be found due to an invalid path", ex);
            return;
        }
        catch (PathTooLongException ex)
        {
            LogManager.LogInformation($"Provided path exceeds max length.", ex);
            return;
        }
        catch (Exception ex)
        {
            LogManager.LogInformation("Error accessing file.", ex);
            return;
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                bookmarks = JsonSerializer.Deserialize<List<Bookmark>>(json) ?? new List<Bookmark>();
            }
        }
        catch (Exception ex)
        {
            LogManager.LogInformation($"Error occured while attempting to deserialize the imports file", ex);
            return;
        }

        bool importSuccessful = true;

        foreach (Bookmark bookmark in bookmarks)
        {
            var result = await _bookmarkService.Import(bookmark, merge);

            if (!result.IsSuccess)
            {
                string message = $"Erorr ocurred while attempting to import bookmark. Error: {result.Message}";
                LogManager.LogError(message, result.Exception);
                MessageHelper.ShowErrorMessage([message]);
                importSuccessful = false;
            }

            BookMarkConflictModel? conflictBookmark = result.Value;
            if (conflictBookmark is not null)
            {
                LogManager.LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Bookmark updated | name changed from '{conflictBookmark.OriginalName}' to '{conflictBookmark.UpdatedName}' for URL '{conflictBookmark.Url}'");
            }
        }

        if (importSuccessful)
        {
            MessageHelper.ShowSuccessMessage(["Bookmarks imported successfully!"]);
        }
    }
}
