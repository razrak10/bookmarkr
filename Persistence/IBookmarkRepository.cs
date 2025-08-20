using bookmarkr.ExecutionResult;

namespace bookmarkr.Persistence
{
    public interface IBookmarkRepository
    {
        Task<ExecutionResult<Bookmark>> AddAsync(Bookmark bookmark);
        Task<ExecutionResult<Bookmark>> DeleteAsync(Bookmark bookmarkToDelete);
        Task<ExecutionResult<IEnumerable<Bookmark>>> FindAllAsync(bool isTrackingChanges);
        Task<ExecutionResult<Bookmark>> FindBookmarkById(int id, bool isTrackingChanges);
        Task<ExecutionResult<Bookmark>> FindBookmarkByName(string name, bool isTrackingChanges);
        Task<ExecutionResult<Bookmark>> FindBookmarkByUrl(string url, bool isTrackingChanges);
        Task<ExecutionResult<IEnumerable<Bookmark>>> FindBookmarksByCategory(string category, bool isTrackingChanges);
        Task<ExecutionResult<Bookmark>> UpdateAsync(Bookmark updatedBookmark);
        Task<int> SaveChangesAsync();
    }
}