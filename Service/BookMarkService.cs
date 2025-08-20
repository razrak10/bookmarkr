using bookmarkr.ExecutionResult;
using bookmarkr.Models;
using bookmarkr.Persistence;
using bookmarkr.Service;
using bookmarkr.ServiceAgent;
using System.Text.Json;

namespace bookmarkr;

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _repository;
    private readonly IBookmarkrLookupServiceAgent _lookupServiceAgent;

    public BookmarkService(IBookmarkRepository repository, IBookmarkrLookupServiceAgent serviceAgent)
    {
        _repository = repository;
        _lookupServiceAgent = serviceAgent;
    }

    public async Task<ExecutionResult<bool>> AddLinkAsync(string name, string url, string category)
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

        ExecutionResult<Bookmark> addResult = await _repository.AddAsync(new Bookmark
        {
            Name = bookmarkName,
            Url = url,
            Category = category
        });

        if (!addResult.IsSuccess)
        {
            return addResult.ToFailure<bool>();
        }

        return ExecutionResult<bool>.Success(true);
    }

    private async Task<ExecutionResult<string>> GetBookmarkNameFromUrlAsync(string name, string url)
    {
        var lookupResult = await _lookupServiceAgent.GetBookmarkTitle(name, url);

        return lookupResult;
    }

    public async Task<ExecutionResult<bool>> RemoveLinkAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ExecutionResult<bool>.Failure("the `name` for the link is not provided. The expected sytnax is:\", \"bookmarkr link add <name> <url>");
        }

        ExecutionResult<Bookmark> findExecutionResult = await _repository.FindBookmarkByName(name, false);
        if (!findExecutionResult.IsSuccess)
        {
            return findExecutionResult.ToFailure<bool>();
        }

        Bookmark existingBookmark = findExecutionResult.Value!;

        if (existingBookmark is null)
        {
            return ExecutionResult<bool>.Failure($"A bookmark with the name `{name}` does not exist. No removal was performed.");
        }

        ExecutionResult<Bookmark> removeResult = await _repository.DeleteAsync(existingBookmark);

        if (!removeResult.IsSuccess)
        {
            return removeResult.ToFailure<bool>();
        }

        return ExecutionResult<bool>.Success(true);
    }


    public async Task<ExecutionResult<IEnumerable<string>>> GetExistingCategoriesAsync()
    {
        ExecutionResult<IEnumerable<Bookmark>> executionResult = await _repository.FindAllAsync(false);

        if (!executionResult.IsSuccess)
        {
            return executionResult.ToFailure<IEnumerable<string>>();
        }

        IEnumerable<Bookmark>? bookmarks = executionResult.Value;

        if (bookmarks is not null && bookmarks.Any())
        {
            IEnumerable<string> categories = bookmarks
                .Where(book => !string.IsNullOrWhiteSpace(book.Category))
                .Select(book => book.Category!)
                .Distinct();

            return ExecutionResult<IEnumerable<string>>.Success(categories);
        }

        return ExecutionResult<IEnumerable<string>>.Failure("No categories were found.");
    }

    public async Task<ExecutionResult<bool>> ChangeBookmarkCategoryAsync(string url, string category)
    {
        ExecutionResult<Bookmark> executionResult = await _repository.FindBookmarkByUrl(url, isTrackingChanges: true);

        if (!executionResult.IsSuccess)
        {
            return executionResult.ToFailure<bool>();
        }

        Bookmark? existingBookmark = executionResult.Value;

        if (existingBookmark is null)
        {
            return ExecutionResult<bool>.Failure($"No bookmark found with URL: {url}. No category change applied.");
        }

        existingBookmark.Category = category;

        return ExecutionResult<bool>.Success(true);
    }

    public async Task<ExecutionResult<IEnumerable<Bookmark>>> GetBookmarksAsync(bool isTrackingChanges)
    {
        ExecutionResult<IEnumerable<Bookmark>> executionResult = await _repository.FindAllAsync(isTrackingChanges);

        if (!executionResult.IsSuccess)
        {
            return executionResult;
        }

        IEnumerable<Bookmark>? bookmarks = executionResult.Value;

        if (bookmarks is null)
        {
            return ExecutionResult<IEnumerable<Bookmark>>.Failure("Retrieved bookmarks cannot be null.");
        }

        return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
    }

    public async Task<ExecutionResult<Bookmark>> GetBookmarkAsync(string bookmarkName)
    {
        ExecutionResult<Bookmark> executionResult = await _repository.FindBookmarkByName(bookmarkName, isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            return executionResult;
        }

        Bookmark? bookmark = executionResult.Value;

        if (bookmark is null)
        {
            return ExecutionResult<Bookmark>.Failure($"No bookmark found with name: {bookmarkName}");
        }

        return ExecutionResult<Bookmark>.Success(bookmark);
    }

    public async void ExportBookmarksAsync(IEnumerable<Bookmark> bookmarks, FileInfo outputFile, CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputFile.FullName, json, cancellationToken);
    }

    public async Task<ExecutionResult<Bookmark>> Import(Bookmark bookmark, bool merge)
    {
        // Check for existing bookmark
        ExecutionResult<Bookmark> findExecutionResult = await _repository.FindBookmarkByUrl(bookmark.Url, isTrackingChanges: true);

        if (!findExecutionResult.IsSuccess)
        {
            return findExecutionResult.ToFailure<Bookmark>();
        }

        // IF present, update with argument bookmark
        Bookmark? existingBookmark = findExecutionResult.Value;

        if (existingBookmark is not null && merge)
        {
            BookMarkConflictModel conflictModel = new BookMarkConflictModel
            {
                OriginalName = existingBookmark.Name,
                UpdatedName = bookmark.Name,
                Url = bookmark.Url
            };
            existingBookmark.Name = bookmark.Name;

            return ExecutionResult<Bookmark>.Success(existingBookmark);
        }
        else
        {
            ExecutionResult<Bookmark> addExecutionResult = await _repository.AddAsync(bookmark);
            return addExecutionResult;
        }
    }

    public async Task<ExecutionResult<IEnumerable<Bookmark>>> GetBookmarksByCategory(string category)
    {
        ExecutionResult<IEnumerable<Bookmark>> executionResult = await _repository.FindBookmarksByCategory(category, isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            return executionResult;
        }

        IEnumerable<Bookmark>? bookmarks = executionResult.Value;

        if (bookmarks is null || !bookmarks.Any())
        {
            return ExecutionResult<IEnumerable<Bookmark>>.Failure($"No bookmarks found with Category: {category}");
        }

        return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
    }

    public async Task<ExecutionResult<bool>> PrintBookmarks()
    {
        ExecutionResult<IEnumerable<Bookmark>> executionResult = await _repository.FindAllAsync(isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            return executionResult.ToFailure<bool>();
        }

        IEnumerable<Bookmark>? bookmarks = executionResult.Value;

        if (bookmarks is null || !bookmarks.Any())
        {
            return ExecutionResult<bool>.Failure("No bookmarks found. Will not print bookmarks.");
        }

        foreach (Bookmark bookmark in bookmarks ?? Enumerable.Empty<Bookmark>())
        {
            Console.WriteLine(bookmark.ToString());
        }

        return ExecutionResult<bool>.Success(true);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _repository.SaveChangesAsync();
    }
}
