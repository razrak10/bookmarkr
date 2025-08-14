using bookmarkr.ExecutionResult;

namespace bookmarkr.ServiceAgent
{
    internal interface IBookmarkrSyncrServiceAgent
    {
        Task<ExecutionResult<List<Bookmark>>> SyncBookmarksAsync(List<Bookmark> localBookmarks);
    }
}
