using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace bookmarkr.Commands;

public class ExportCommandHandler : AsynchronousCommandLineAction
{
    private readonly BookMarkService _bookmarkService;

    public ExportCommandHandler(BookMarkService bookMarkService)
    {
        _bookmarkService = bookMarkService;
    }

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        FileInfo? outputFile = parseResult.GetValue<FileInfo>("file");
        if (outputFile is not null)
        {
            await OnExportCommand(_bookmarkService, outputFile, cancellationToken);
        }

        return -1;
    }

    private static async Task OnExportCommand(
    BookMarkService bookMarkService, FileInfo outputFile, CancellationToken cancToken)
    {
        try
        {
            Console.WriteLine("Starting export operation...");
            var bookmarks = bookMarkService.GetAll();
            string json = JsonSerializer.Serialize(bookmarks,
            new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputFile.FullName, json, cancToken);
        }
        catch (OperationCanceledException ex)
        {
            string requested = ex.CancellationToken.IsCancellationRequested
            ? "Cancellation was requestd by user"
            : "Cancellation was not requested by user";
            CommandHelper.ShowWarningMessage([$"Operation was cancelled.\n{requested}\nCancellation reason: {ex.Message}"]);
        }
        catch (JsonException ex)
        {
            CommandHelper.ShowErrorMessage([$"Failed to serialize bookmarks to JSON.\nError message {ex.Message}"]);
        }
        catch (UnauthorizedAccessException ex)
        {
            CommandHelper.ShowErrorMessage([$"Insufficient permission to access the file {outputFile.FullName}\nError message {ex.Message}"]);
        }
        catch (DirectoryNotFoundException ex)
        {
            CommandHelper.ShowErrorMessage([$"The file {outputFile.FullName} cannot be found due to an invalid path\nError message {ex.Message}"]);
        }
        catch (PathTooLongException ex)
        {
            CommandHelper.ShowErrorMessage([$"Provided path exceeds max length.\nError message {ex.Message}"]);
        }
        catch (Exception ex)
        {
            CommandHelper.ShowErrorMessage([$"Unknown exception has occured\nError message {ex.Message}"]);
        }
    }
}
