using bookmarkr.ExecutionResult;
using System.Text.RegularExpressions;

namespace bookmarkr.ServiceAgent
{
    public class BookmarkrLookupServiceAgent : IBookmarkrLookupServiceAgent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BookmarkrLookupServiceAgent(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ExecutionResult<string>> GetBookmarkTitle(string name, string url)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                string response = await httpClient.GetStringAsync(url);

                var match = Regex.Match(response,
                    @"<title[^>]*>\s*([^<]*?)\s*</title>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                {
                    return ExecutionResult<string>.Success(match.Groups[1].Value.Trim());
                }

                return ExecutionResult<string>.Success(name);
            }
            catch (HttpRequestException ex)
            {
                return ExecutionResult<string>.Failure($"HttpRequest exception occured while attempting to lookup the bookmark URL. Status code: {ex.StatusCode}",
                    ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                return ExecutionResult<string>.Failure("Timeout occurred while attempting to lookup the bookmark URL", ex);
            }
            catch (TaskCanceledException ex)
            {
                return ExecutionResult<string>.Failure("Operation was cancelled while attempting to lookup the bookmark URL", ex);
            }
            catch (Exception ex)
            {
                return ExecutionResult<string>.Failure("Error occurred while attempting to lookup the bookmark URL", ex);
            }
        }
    }
}