using bookmarkr.ExecutionResult;
using System.Linq.Expressions;

namespace bookmarkr.Persistence
{
    internal interface IBookmarkRepository
    {
        Task<ExecutionResult<Bookmark>> AddAsync(Bookmark bookmark);
        Task<ExecutionResult<Bookmark>> DeleteAsync(Bookmark bookmarkToDelete);
        Task<ExecutionResult<IEnumerable<Bookmark>>> FindAllAsync();
        Task<ExecutionResult<IQueryable<T>>> FindByConditionAsync<T>(Expression<Func<T, bool>> expression) where T : class;
        Task<ExecutionResult<Bookmark>> UpdateAsync(Bookmark updatedBookmark);
    }
}