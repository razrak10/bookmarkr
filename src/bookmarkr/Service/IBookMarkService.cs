using bookmarkr.ExecutionResult;

namespace bookmarkr.Service;

public interface IBookmarkService
{
    Task<ExecutionResult<bool>> AddBookmarkAsync(string name, string url, string category);
    Task<ExecutionResult<bool>> RemoveLinkAsync(string name);
    Task<ExecutionResult<bool>> ChangeBookmarkCategoryAsync(string url, string category);
    void ExportBookmarksAsync(IEnumerable<Bookmark> bookmarks, FileInfo outputFile, CancellationToken cancellationToken = default);
    Task<ExecutionResult<Bookmark>> GetBookmarkAsync(string bookmarkName);
    Task<ExecutionResult<IEnumerable<Bookmark>>> GetBookmarksAsync(bool isTrackingChanges);
    Task<ExecutionResult<IEnumerable<Bookmark>>> GetBookmarksByCategory(string category);
    Task<ExecutionResult<IEnumerable<string>>> GetExistingCategoriesAsync();
    Task<ExecutionResult<Bookmark>> Import(Bookmark bookmark, bool merge);
    Task<ExecutionResult<bool>> PrintBookmarks();
    Task<int> SaveChangesAsync();
}
