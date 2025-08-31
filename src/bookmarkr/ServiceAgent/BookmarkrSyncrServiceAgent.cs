using bookmarkr.ExecutionResult;
using bookmarkr.Logger;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace bookmarkr.ServiceAgent
{
    public class BookmarkrSyncrServiceAgent : IBookmarkrSyncrServiceAgent
    {
        private readonly IHttpClientFactory _clientFactory;

        public BookmarkrSyncrServiceAgent(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<ExecutionResult<List<Bookmark>>> SyncBookmarksAsync(List<Bookmark> localBookmarks)
        {
            if (!localBookmarks.Any())
            {
                return ExecutionResult<List<Bookmark>>.Failure("No bookmarks provided");
            }

            const string defaultErrorMessage = "An error occured when attempting to sync bookmarks.";
            try
            {
                List<Bookmark> mergedBookmarks = new List<Bookmark>();
                string serializedRetrievedBookmarks = JsonSerializer.Serialize(localBookmarks);
                var content = new StringContent(serializedRetrievedBookmarks, Encoding.UTF8, "application/json");

                using var httpClient = _clientFactory.CreateClient("bookmarkrSyncr");
                var response = await httpClient.PostAsync("sync", content);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    mergedBookmarks = await JsonSerializer.DeserializeAsync<List<Bookmark>>(
                        await response?.Content?.ReadAsStreamAsync(),
                        options);

                    if (mergedBookmarks is not null && mergedBookmarks.Any())
                    {
                        LogManager.LogInformation("Bookmarks synchronized successfully.");
                        return ExecutionResult<List<Bookmark>>.Success(mergedBookmarks ?? new List<Bookmark>());
                    }
                }
                else
                {
                    string errorMessage = response.StatusCode switch
                    {
                        HttpStatusCode.NotFound => $"Resource not found: {response.StatusCode}",
                        HttpStatusCode.Unauthorized => $"Unauthorized access: {response.StatusCode}",
                        _ => $"Failed to sync bookmarks: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}"
                    };

                    LogManager.LogError(errorMessage);
                    return ExecutionResult<List<Bookmark>>.Failure(errorMessage);
                }

                return ExecutionResult<List<Bookmark>>.Failure(defaultErrorMessage);

            }
            catch (JsonException ex)
            {
                LogManager.LogError($"Error occured while attempting to serialize bookmarks to JSON.", ex);
                return ExecutionResult<List<Bookmark>>.Failure("Failed to serialize bookmarks to JSON format.", ex);
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Unexpected error during bookmark synchronization.", ex);
                return ExecutionResult<List<Bookmark>>.Failure("An unexpected error occurred during synchronization.", ex);
            }
        }
    }
}
