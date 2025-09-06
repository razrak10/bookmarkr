using bookmarkr.ExecutionResult;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq.Expressions;

namespace bookmarkr.Persistence
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly BookmarkrDbContext _context;

        public BookmarkRepository(BookmarkrDbContext context)
        {
            _context = context;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<ExecutionResult<Bookmark>> AddAsync(Bookmark bookmark)
        {
            await _semaphore.WaitAsync();

            try
            {
                await _context.Bookmarks.AddAsync(bookmark);
                await _context.SaveChangesAsync();
                return ExecutionResult<Bookmark>.Success(bookmark);
            }
            catch (DbUpdateException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Database Updater Error occured while adding bookmark to database.", ex);
            }
            catch (DbException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Database error occured while adding bookmark to database.", ex);
            }
            catch (Exception ex)
            {
                return ExecutionResult<Bookmark>.Failure("Error occurred while adding bookmark to database.", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task<ExecutionResult<Bookmark>> DeleteAsync(Bookmark bookmarkToDelete)
        {
            await _semaphore.WaitAsync();

            try
            {
                _context.Bookmarks.Remove(bookmarkToDelete);
                await _context.SaveChangesAsync();

                return ExecutionResult<Bookmark>.Success(bookmarkToDelete);
            }
            catch (DbUpdateException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Database Update Error occured while deleting bookmark from database.", ex);
            }
            catch (DbException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Database error occured while deleting bookmark from database.", ex);
            }
            catch (Exception ex)
            {
                return ExecutionResult<Bookmark>.Failure("Error occured while deleting bookmark from database.", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<ExecutionResult<IEnumerable<Bookmark>>> FindAllAsync(bool isTrackingChanges)
        {
            try
            {
                List<Bookmark> bookmarks;

                if (isTrackingChanges)
                {
                    bookmarks = await _context.Bookmarks
                        .OrderBy(x => x.Id)
                        .ThenBy(x => x.Name)
                        .ToListAsync();
                }
                else
                {
                    bookmarks = await _context.Bookmarks
                            .OrderBy(x => x.Id)
                            .ThenBy(x => x.Name)
                            .AsNoTracking()
                            .ToListAsync();
                }

                return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
            }
            catch (DbException exception)
            {
                return ExecutionResult<IEnumerable<Bookmark>>.Failure("Database error occured.", exception);
            }
            catch (Exception exception)
            {
                return ExecutionResult<IEnumerable<Bookmark>>.Failure("Unexpected error occured.", exception);
            }
        }

        public async Task<ExecutionResult<Bookmark>> UpdateAsync(Bookmark updatedBookmark)
        {
            await _semaphore.WaitAsync();

            try
            {
                _context.Bookmarks.Update(updatedBookmark);

                await _context.SaveChangesAsync();

                return ExecutionResult<Bookmark>.Success(updatedBookmark);
            }
            catch (DbUpdateException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Error occured while updating bookmark in database.", ex);
            }
            catch (DbException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Database error occured.", ex);
            }
            catch (Exception ex)
            {
                return ExecutionResult<Bookmark>.Failure("Unexpected error occured.", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<ExecutionResult<Bookmark>> FindBookmarkById(int id, bool isTrackingChanges)
        {
            ExecutionResult<IQueryable<Bookmark>> executionResult = FindByConditionAsync<Bookmark>(b => b.Id == id, isTrackingChanges);

            if (!executionResult.IsSuccess)
            {
                return executionResult.ToFailure<Bookmark>();
            }

            Bookmark? bookmark = await executionResult.Value!.SingleOrDefaultAsync();

            return ExecutionResult<Bookmark>.Success(bookmark!);
        }

        public async Task<ExecutionResult<Bookmark>> FindBookmarkByName(string name, bool isTrackingChanges)
        {
            ExecutionResult<IQueryable<Bookmark>> executionResult = FindByConditionAsync<Bookmark>(b => b.Name.ToLower() == name.ToLower(), isTrackingChanges);

            if (!executionResult.IsSuccess)
            {
                return executionResult.ToFailure<Bookmark>();
            }

            Bookmark? bookmark = await executionResult.Value!.SingleOrDefaultAsync();

            return ExecutionResult<Bookmark>.Success(bookmark!);
        }

        public async Task<ExecutionResult<Bookmark>> FindBookmarkByUrl(string url, bool isTrackingChanges)
        {
            ExecutionResult<IQueryable<Bookmark>> executionResult = FindByConditionAsync<Bookmark>(b => b.Url.ToLower() == url.ToLower(), isTrackingChanges);

            if (!executionResult.IsSuccess)
            {
                return executionResult.ToFailure<Bookmark>();
            }

            Bookmark? bookmark = await executionResult.Value!.SingleOrDefaultAsync();

            return ExecutionResult<Bookmark>.Success(bookmark!);
        }

        public async Task<ExecutionResult<IEnumerable<Bookmark>>> FindBookmarksByCategory(string category, bool isTrackingChanges)
        {
            ExecutionResult<IQueryable<Bookmark>> executionResult = FindByConditionAsync<Bookmark>(b =>
            string.Equals(b.Category, category), isTrackingChanges);

            if (!executionResult.IsSuccess)
            {
                return executionResult.ToFailure<IEnumerable<Bookmark>>();
            }

            IEnumerable<Bookmark> bookmarks = await executionResult.Value!.ToListAsync();

            return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
        }

        private ExecutionResult<IQueryable<T>> FindByConditionAsync<T>(
            Expression<Func<T, bool>> expression,
            bool isTrackingChanges) where T : class
        {
            try
            {
                IQueryable<T> queryableResult;
                if (isTrackingChanges)
                {
                    queryableResult = _context.Set<T>().Where(expression);
                }
                else
                {
                    queryableResult = _context.Set<T>().Where(expression).AsNoTracking();
                }

                return ExecutionResult<IQueryable<T>>.Success(queryableResult);
            }
            catch (DbException ex)
            {
                return ExecutionResult<IQueryable<T>>.Failure("Database error occured.", ex);
            }
            catch (Exception ex)
            {
                return ExecutionResult<IQueryable<T>>.Failure("Unexpected error occured.", ex);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
