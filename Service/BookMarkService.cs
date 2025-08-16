using bookmarkr.ExecutionResult;
using bookmarkr.Models;
using bookmarkr.Persistence;
using bookmarkr.Service;
using System.Text.Json;

namespace bookmarkr;

public class BookMarkService : IBookMarkService
{
    private readonly BookmarkRepository _repository;

    public BookMarkService(BookmarkRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExecutionResult<bool>> AddLinkAsync(string name, string url, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ExecutionResult<bool>.Failure("the `name` for the link is not provided. The expected sytnax is:\", \"bookmarkr link add <name> <url>");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return ExecutionResult<bool>.Failure("the `url` for the link is not provided. The expected sytnax is:\", \"bookmarkr link add <name> <url>");
        }

        var findExecutionResult = await _repository.FindBookmarkByName(name, false);
        if (!findExecutionResult.IsSuccess)
        {
            return findExecutionResult.ToFailure<bool>();
        }

        Bookmark existingBookmark = findExecutionResult.Value;

        if (existingBookmark is not null)
        {
            return ExecutionResult<bool>.Failure($"A bookmark with the name `{name}` already exists. It will thus not be added");
        }

        await _repository.AddAsync(new Bookmark
        {
            Name = name,
            Url = url,
            Category = category
        });

        return ExecutionResult<bool>.Success(true);
    }


    public async Task<ExecutionResult<IEnumerable<string>>> GetExistingCategoriesAsync()
    {
        var executionResult = await _repository.FindAllAsync(false);

        if (!executionResult.IsSuccess)
        {
            return executionResult.ToFailure<IEnumerable<string>>();
        }

        var bookmarks = executionResult.Value;

        if (bookmarks is not null && bookmarks.Any())
        {
            var categories = bookmarks
                .Where(book => !string.IsNullOrWhiteSpace(book.Category))
                .Select(book => book.Category!)
                .Distinct();

            return ExecutionResult<IEnumerable<string>>.Success(categories);
        }

        return ExecutionResult<IEnumerable<string>>.Failure("No categories were found.");
    }

    public async Task<ExecutionResult<bool>> ChangeBookmarkCategoryAsync(string url, string category)
    {
        var executionResult = await _repository.FindBookmarkByUrl(url, isTrackingChanges: true);

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
        var executionResult = await _repository.FindAllAsync(isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            return executionResult;
        }

        IEnumerable<Bookmark>? bookmarks = executionResult.Value;

        if (bookmarks is null || !bookmarks.Any())
        {
            return ExecutionResult<IEnumerable<Bookmark>>.Failure("No bookmarks found.");
        }

        return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
    }

    public async Task<ExecutionResult<Bookmark>> GetBookmarkAsync(string bookmarkName)
    {
        var executionResult = await _repository.FindBookmarkByName(bookmarkName, isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            return executionResult;
        }

        var bookmark = executionResult.Value;

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

    public async Task<ExecutionResult<BookMarkConflictModel>> Import(Bookmark bookmark, bool merge)
    {
        // Check for existing bookmark
        var executionResult = await _repository.FindBookmarkByUrl(bookmark.Url, isTrackingChanges: true);

        if (!executionResult.IsSuccess)
        {
            return executionResult.ToFailure<BookMarkConflictModel>();
        }

        // IF present, update with argument bookmark
        var existingBookmark = executionResult.Value;

        if (existingBookmark is not null && merge)
        {
            var conflictModel = new BookMarkConflictModel
            {
                OriginalName = existingBookmark.Name,
                UpdatedName = bookmark.Name,
                Url = bookmark.Url
            };
            existingBookmark.Name = bookmark.Name;

            return ExecutionResult<BookMarkConflictModel>.Success(conflictModel);
        }
        else
        {
            await _repository.AddAsync(bookmark);
            return ExecutionResult<BookMarkConflictModel>.Success(null!);
        }
    }

    public async Task<ExecutionResult<IEnumerable<Bookmark>>> GetBookmarksByCategory(string category)
    {
        var executionResult = await _repository.FindBookmarksByCategory(category, isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            return executionResult;
        }

        var bookmarks = executionResult.Value;

        if (bookmarks is null || !bookmarks.Any())
        {
            return ExecutionResult<IEnumerable<Bookmark>>.Failure($"No bookmarks found with Category: {category}");
        }

        return ExecutionResult<IEnumerable<Bookmark>>.Success(bookmarks);
    }

    public async Task<ExecutionResult<bool>> PrintBookmarks()
    {
        var executionResult = await _repository.FindAllAsync(isTrackingChanges: false);

        if (!executionResult.IsSuccess)
        {
            return executionResult.ToFailure<bool>();
        }

        IEnumerable<Bookmark>? bookmarks = executionResult.Value;

        if (bookmarks is null || !bookmarks.Any())
        {
            return ExecutionResult<bool>.Failure("No bookmarks found. Will not print bookmarks.");
        }

        foreach (var bookmark in bookmarks ?? Enumerable.Empty<Bookmark>())
        {
            Console.WriteLine(bookmark.ToString());
        }

        return ExecutionResult<bool>.Success(true);
    }

}
