using bookmarkr.ExecutionResult;

namespace bookmarkr.ServiceAgent
{
    public interface IBookmarkrLookupServiceAgent
    {
        public Task<ExecutionResult<string>> GetBookmarkTitle(string name, string url);
    }
}
