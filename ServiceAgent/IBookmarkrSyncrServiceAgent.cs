using bookmarkr.ExecutionResult;

namespace bookmarkr.ServiceAgent
{
    public interface IBookmarkrSyncrServiceAgent
    {
        Task<ExecutionResult<List<Bookmark>>> SyncBookmarksAsync(List<Bookmark> localBookmarks);
    }
}
