using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using System.Text.Json;

namespace bookmarkr.Commands.Sync;

public class SyncCommandHandler : AsynchronousCommandLineAction
{
    private readonly BookMarkService _bookmarkService;
    private readonly IHttpClientFactory _clientFactory;

    public SyncCommandHandler(IHttpClientFactory clientFactory, BookMarkService bookMarkService)
    {
        _clientFactory = clientFactory;
        _bookmarkService = bookMarkService;
    }

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        await OnSyncCommand();
        return 0;
    }

    private async Task OnSyncCommand()
    {
        IReadOnlyCollection<Bookmark> retrievedBookmarks = _bookmarkService.GetAll();

        try
        {
            string serializedRetrievedBookmarks = JsonSerializer.Serialize(retrievedBookmarks);
            var content = new StringContent(serializedRetrievedBookmarks, Encoding.UTF8, "application/json");

            var httpClient = _clientFactory.CreateClient("bookmarkrSyncr");
            var response = await httpClient.PostAsync("sync", content);

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                var mergedBookmarks = await JsonSerializer.DeserializeAsync<List<Bookmark>>(
                    await response.Content.ReadAsStreamAsync(),
                    options
                );

                if (mergedBookmarks is not null && mergedBookmarks.Any(b => b is not null))
                {
                    _bookmarkService.ClearAll();
                    _bookmarkService.Import(mergedBookmarks, merge: true);
                }
                LogManager.LogInformation("Successfully synced bookmarks");
            }
            else
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        LogManager.LogError("Resource not found");
                        break;
                    case System.Net.HttpStatusCode.Unauthorized:
                        LogManager.LogError("Unauthorized access");
                        break;
                    default:
                        var error = await response.Content.ReadAsStringAsync();
                        LogManager.LogError($"Failed to sync bookmarks | {error}");
                        break;
                }
            }
        }
        catch (JsonException ex)
        {
            CommandHelper.ShowErrorMessage([$"Failed to serialize bookmarks to JSON format.\nError message: {ex.Message}"]);
        }
        catch (Exception ex)
        {
            CommandHelper.ShowErrorMessage([$"Unknown exception has occured\nError message: {ex.Message}"]);
        }
    }
}
