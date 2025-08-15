
using bookmarkr.ExecutionResult;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.CommandLine;
using System.Data.Common;
using System.Linq.Expressions;

namespace bookmarkr.Persistence
{
    internal class BookmarkRepository : IBookmarkRepository
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
                if (bookmark is not null)
                {
                    await _context.Bookmarks.AddAsync(bookmark);
                    await _context.SaveChangesAsync();
                    return ExecutionResult<Bookmark>.Success(bookmark);
                }

                return ExecutionResult<Bookmark>.Failure("Bookmark to delete was null or empty.");
            }
            catch (DbUpdateException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Error occured while adding bookmark to database.", ex);
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
        public async Task<ExecutionResult<Bookmark>> DeleteAsync(Bookmark bookmarkToDelete)
        {
            await _semaphore.WaitAsync();

            try
            {

                if (bookmarkToDelete != null)
                {
                    _context.Bookmarks.Remove(bookmarkToDelete);
                    await _context.SaveChangesAsync();

                    return ExecutionResult<Bookmark>.Success(bookmarkToDelete);
                }

                return ExecutionResult<Bookmark>.Failure("Bookmark to delete was null or empty.");
            }
            catch (DbUpdateException ex)
            {
                return ExecutionResult<Bookmark>.Failure("Error occured while deleting bookmark from database.", ex);
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

        //public async Task<ExecutionResult<Bookmark>> DeleteAsync(Bookmark bookmarkToDelete)
        //{
        //    await _semaphore.WaitAsync();

        //    try
        //    {
        //        ExecutionResult<IQueryable<Bookmark>> executionResult = await FindByConditionAsync<Bookmark>(e => e.Id == id);

        //        if (!executionResult.IsSuccess)
        //        {
        //            return executionResult.ToFailure<Bookmark>();
        //        }

        //        Bookmark bookmarkToDelete = await executionResult.Value.SingleOrDefaultAsync();

        //        if (bookmarkToDelete != null)
        //        {
        //            _context.Bookmarks.Remove(bookmarkToDelete);
        //        }

        //        await _context.SaveChangesAsync();

        //        return ExecutionResult<Bookmark>.Failure($"More than 1 bookmarkToDelete found with {id}");
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        return ExecutionResult<Bookmark>.Failure("Error occured while deleting bookmarkToDelete from database.", ex);
        //    }
        //    catch (DbException ex)
        //    {
        //        return ExecutionResult<Bookmark>.Failure("Database error occured.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ExecutionResult<Bookmark>.Failure("Unexpected error occured.", ex);
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}

        public async Task<ExecutionResult<IEnumerable<Bookmark>>> FindAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                IEnumerable<Bookmark> bookmarks = await _context.Bookmarks
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .ThenBy(x => x.Name)
                    .ToListAsync();

                if (bookmarks != null && bookmarks.Any())
                {
                    return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
                }

                return ExecutionResult<IEnumerable<Bookmark>>.Failure("No bookmarks retrieved from the bookmarks database.");
            }
            catch (DbException exception)
            {
                return ExecutionResult<IEnumerable<Bookmark>>.Failure("Database error occured.", exception);
            }
            catch (Exception exception)
            {
                return ExecutionResult<IEnumerable<Bookmark>>.Failure("Unexpected error occured.", exception);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<ExecutionResult<IQueryable<T>>> FindByConditionAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            await _semaphore.WaitAsync();

            try
            {
                IQueryable<T> queryableResult = _context.Set<T>().Where(expression);

                if (queryableResult is not null)
                {
                    return ExecutionResult<IQueryable<T>>.Success(queryableResult);
                }

                return ExecutionResult<IQueryable<T>>.Failure($"No bookmarks found desired condition.");
            }
            catch (DbException ex)
            {
                return ExecutionResult<IQueryable<T>>.Failure("Database error occured.", ex);
            }
            catch (Exception ex)
            {
                return ExecutionResult<IQueryable<T>>.Failure("Unexpected error occured.", ex);
            }
            finally
            {
                _semaphore.Release();
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

        //public async Task<ExecutionResult<Bookmark>> UpdateAsync(Bookmark updatedBookmark)
        //{
        //    await _semaphore.WaitAsync();

        //    try
        //    {
        //        ExecutionResult<IQueryable<Bookmark>> executionResult = await FindByConditionAsync<Bookmark>(b => b.Id == updatedBookmark.Id);

        //        if (!executionResult.IsSuccess)
        //        {
        //            return executionResult.ToFailure<Bookmark>();
        //        }

        //        Bookmark existingBookmark = await executionResult.Value.SingleOrDefaultAsync();

        //        if (existingBookmark is null)
        //        {
        //            return ExecutionResult<Bookmark>.Failure("More than 1 bookmarkToDelete found with the same id.");
        //        }

        //        existingBookmark.Name = updatedBookmark.Name;
        //        existingBookmark.Category = updatedBookmark.Category;
        //        existingBookmark.Url = updatedBookmark.Url;
        //        existingBookmark.UpdatedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();

        //        return ExecutionResult<Bookmark>.Success(existingBookmark);
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        return ExecutionResult<Bookmark>.Failure("Error occured while updating bookmarkToDelete with id: {}in database.", ex);
        //    }
        //    catch (DbException ex)
        //    {
        //        return ExecutionResult<Bookmark>.Failure("Database error occured.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ExecutionResult<Bookmark>.Failure("Unexpected error occured.", ex);
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}
    }
}
