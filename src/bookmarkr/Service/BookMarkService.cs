using bookmarkr.ExecutionResult;
using bookmarkr.Models;
using bookmarkr.Persistence.Abstractions;
using bookmarkr.Service;
using bookmarkr.ServiceAgent;
using System.Text.Json;

namespace bookmarkr;

/// <summary>
/// Service for managing bookmark operations.
/// </summary>
public class BookmarkService : IBookmarkService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookmarkrLookupServiceAgent _lookupServiceAgent;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookmarkService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="serviceAgent">The service agent for bookmark lookups.</param>
    public BookmarkService(IUnitOfWork unitOfWork, IBookmarkrLookupServiceAgent serviceAgent)
    {
        _unitOfWork = unitOfWork;
        _lookupServiceAgent = serviceAgent;
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<bool>> AddBookmarkAsync(string name, string url, string category)
    {
        string bookmarkName = string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            return ExecutionResult<bool>.Failure("the `name` for the link is not provided. The expected sytnax is:\", \"bookmarkr link add <name> <url>");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return ExecutionResult<bool>.Failure("the `url` for the link is not provided. The expected sytnax is:\", \"bookmarkr link add <name> <url>");
        }

        var nameResult = await GetBookmarkNameFromUrlAsync(name, url);

        if (!nameResult.IsSuccess)
        {
            bookmarkName = "Unnamed bookmark";
        }
        else if (nameResult.IsSuccess && string.Equals(nameResult.Value, name))
        {
            bookmarkName = name;
        }
        else
        {
            bookmarkName = nameResult.Value!;
        }

        try
        {
            await _unitOfWork.Bookmarks.AddAsync(new Bookmark
            {
                Name = bookmarkName,
                Url = url,
                Category = category
            });

            await _unitOfWork.SaveChangesAsync();
            return ExecutionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ExecutionResult<bool>.Failure($"Error occurred while adding bookmark: {ex.Message}");
        }
    }

    private async Task<ExecutionResult<string>> GetBookmarkNameFromUrlAsync(string name, string url)
    {
        var lookupResult = await _lookupServiceAgent.GetBookmarkTitle(name, url);

        return lookupResult;
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<bool>> RemoveLinkAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ExecutionResult<bool>.Failure("the `name` for the link is not provided. The expected sytnax is:\", \"bookmarkr link add <name> <url>");
        }

        try
        {
            var existingBookmark = await _unitOfWork.Bookmarks.GetByNameAsync(name);

            if (existingBookmark is null)
            {
                return ExecutionResult<bool>.Failure($"A bookmark with the name `{name}` does not exist. No removal was performed.");
            }

            _unitOfWork.Bookmarks.Delete(existingBookmark);
            await _unitOfWork.SaveChangesAsync();

            return ExecutionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ExecutionResult<bool>.Failure($"Error occurred while removing bookmark: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<IEnumerable<string>>> GetExistingCategoriesAsync()
    {
        try
        {
            var bookmarks = await _unitOfWork.Bookmarks.GetAllAsync();

            if (bookmarks.Any())
            {
                IEnumerable<string> categories = bookmarks
                    .Where(book => !string.IsNullOrWhiteSpace(book.Category))
                    .Select(book => book.Category!)
                    .Distinct();

                return ExecutionResult<IEnumerable<string>>.Success(categories);
            }

            return ExecutionResult<IEnumerable<string>>.Failure("No categories were found.");
        }
        catch (Exception ex)
        {
            return ExecutionResult<IEnumerable<string>>.Failure($"Error occurred while retrieving categories: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<bool>> ChangeBookmarkCategoryAsync(string url, string category)
    {
        try
        {
            var existingBookmark = await _unitOfWork.Bookmarks.GetByUrlAsync(url);

            if (existingBookmark is null)
            {
                return ExecutionResult<bool>.Failure($"No bookmark found with URL: {url}. No category change applied.");
            }

            existingBookmark.Category = category;
            _unitOfWork.Bookmarks.Update(existingBookmark);
            await _unitOfWork.SaveChangesAsync();

            return ExecutionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ExecutionResult<bool>.Failure($"Error occurred while changing bookmark category: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<IEnumerable<Bookmark>>> GetBookmarksAsync(bool isTrackingChanges)
    {
        try
        {
            var bookmarks = await _unitOfWork.Bookmarks.GetAllAsync();

            return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
        }
        catch (Exception ex)
        {
            return ExecutionResult<IEnumerable<Bookmark>>.Failure($"Error occurred while retrieving bookmarks: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<Bookmark>> GetBookmarkAsync(string bookmarkName)
    {
        try
        {
            var bookmark = await _unitOfWork.Bookmarks.GetByNameAsync(bookmarkName);

            if (bookmark is null)
            {
                return ExecutionResult<Bookmark>.Failure($"No bookmark found with name: {bookmarkName}");
            }

            return ExecutionResult<Bookmark>.Success(bookmark);
        }
        catch (Exception ex)
        {
            return ExecutionResult<Bookmark>.Failure($"Error occurred while retrieving bookmark: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async void ExportBookmarksAsync(IEnumerable<Bookmark> bookmarks, FileInfo outputFile, CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputFile.FullName, json, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<Bookmark>> Import(Bookmark bookmark, bool merge)
    {
        try
        {
            // Check for existing bookmark
            var existingBookmark = await _unitOfWork.Bookmarks.GetByUrlAsync(bookmark.Url);

            // If present, update with argument bookmark
            if (existingBookmark is not null && merge)
            {
                BookMarkConflictModel conflictModel = new BookMarkConflictModel
                {
                    OriginalName = existingBookmark.Name,
                    UpdatedName = bookmark.Name,
                    Url = bookmark.Url
                };
                existingBookmark.Name = bookmark.Name;
                _unitOfWork.Bookmarks.Update(existingBookmark);
                await _unitOfWork.SaveChangesAsync();

                return ExecutionResult<Bookmark>.Success(existingBookmark);
            }
            else
            {
                await _unitOfWork.Bookmarks.AddAsync(bookmark);
                await _unitOfWork.SaveChangesAsync();

                return ExecutionResult<Bookmark>.Success(bookmark);
            }
        }
        catch (Exception ex)
        {
            return ExecutionResult<Bookmark>.Failure($"Error occurred while importing bookmark: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<IEnumerable<Bookmark>>> GetBookmarksByCategory(string category)
    {
        try
        {
            var bookmarks = await _unitOfWork.Bookmarks.GetByCategoryAsync(category);

            if (!bookmarks.Any())
            {
                return ExecutionResult<IEnumerable<Bookmark>>.Failure($"No bookmarks found with Category: {category}");
            }

            return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
        }
        catch (Exception ex)
        {
            return ExecutionResult<IEnumerable<Bookmark>>.Failure($"Error occurred while retrieving bookmarks by category: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<bool>> PrintBookmarks()
    {
        try
        {
            var bookmarks = await _unitOfWork.Bookmarks.GetAllAsync();

            if (!bookmarks.Any())
            {
                return ExecutionResult<bool>.Failure("No bookmarks found. Will not print bookmarks.");
            }

            foreach (Bookmark bookmark in bookmarks)
            {
                Console.WriteLine(bookmark.ToString());
            }

            return ExecutionResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ExecutionResult<bool>.Failure($"Error occurred while printing bookmarks: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        return await _unitOfWork.SaveChangesAsync();
    }
}
