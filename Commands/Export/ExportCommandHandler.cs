using bookmarkr.Helpers;
using bookmarkr.Logger;
using System.CommandLine;
using System.Text.Json;

namespace bookmarkr.Commands;

public class ExportCommandHandler
{
    private readonly BookmarkService _bookmarkService;

    public ExportCommandHandler(BookmarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        FileInfo? outputFile = parseResult.GetValue<FileInfo>("file");
        if (outputFile is not null)
        {
            await OnExportCommand(outputFile, cancellationToken);
        }

        return -1;
    }

    private async Task OnExportCommand(FileInfo outputFile, CancellationToken cancToken)
    {
        try
        {
            Console.WriteLine("Starting export operation...");

            ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> result = await _bookmarkService.GetBookmarksAsync(false);

            if (!result.IsSuccess)
            {
                string message = $"Error occured when retrieving all bookmarks. Error: {result.Message}";
                LogManager.LogError(message, result.Exception);
                MessageHelper.ShowErrorMessage([message]);
                return;
            }

            IEnumerable<Bookmark>? bookmarks = result.Value;

            if (bookmarks!.Any())
            {

                string json = JsonSerializer.Serialize(result.Value,
                new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(outputFile.FullName, json, cancToken);
            }
        }
        catch (OperationCanceledException ex)
        {
            string requested = ex.CancellationToken.IsCancellationRequested
            ? "Cancellation was requestd by user"
            : "Cancellation was not requested by user";
            MessageHelper.ShowWarningMessage([$"Operation was cancelled.\n{requested}\nCancellation reason: {ex.Message}"]);
        }
        catch (JsonException ex)
        {
            MessageHelper.ShowErrorMessage([$"Failed to serialize bookmarks to JSON.\nError message {ex.Message}"]);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageHelper.ShowErrorMessage([$"Insufficient permission to access the file {outputFile.FullName}\nError message {ex.Message}"]);
        }
        catch (DirectoryNotFoundException ex)
        {
            MessageHelper.ShowErrorMessage([$"The file {outputFile.FullName} cannot be found due to an invalid path\nError message {ex.Message}"]);
        }
        catch (PathTooLongException ex)
        {
            MessageHelper.ShowErrorMessage([$"Provided path exceeds max length.\nError message {ex.Message}"]);
        }
        catch (Exception ex)
        {
            MessageHelper.ShowErrorMessage([$"Unknown exception has occured\nError message {ex.Message}"]);
        }
    }
}
