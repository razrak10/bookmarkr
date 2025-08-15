using System;
using System.Text.Json;
using bookmarkr.Helpers;
using bookmarkr.Models;
using bookmarkr.Service;

namespace bookmarkr;

public class BookMarkService : IBookMarkService
{
    private readonly HashSet<string> _categories = new()
    {
        "Cars",
        "Tech",
        "SocialMedia",
        "Cooking"
    };

    private readonly List<Bookmark> _existingBookmarks = new List<Bookmark>
    {
        new Bookmark {
            Name = "First",
            Category = "Cars",
            Url = "https://www.lol.com"
        },
        new Bookmark {
            Name = "Second",
            Category = "Tech",
            Url = "https://www.seocnd.com"
        },
        new Bookmark {
            Name = "First",
            Category = "Tech",
            Url = "https://www.lol.com"
        },
        new Bookmark {
            Name = "First",
            Category = "Cooking",
            Url = "https://www.lol.com"
        },
        new Bookmark {
            Name = "First",
            Category = "SocialMedia",
            Url = "https://www.lol.com"
        },
    };

    public HashSet<string> Categories => _categories;
    public List<Bookmark> ExistingBookmarks => _existingBookmarks;

    public void AddLink(string name, string url, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            CommandHelper.ShowErrorMessage(["the `name` for the link is not provided. The expected sytnax is:", "bookmarkr link add <name> <url>"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            CommandHelper.ShowErrorMessage(["the `url` for the link is not provided. The expected sytnax is:", "bookmarkr link add <name> <url>"]);
            return;
        }
        if (ExistingBookmarks.Any(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            CommandHelper.ShowErrorMessage(["A link with the name `{name}` already exists. It will thus not be added", $"To update the existing link, use the command: bookmarkr link update `{name}` `{url}`"]);
            return;
        }

        ExistingBookmarks.Add(new Bookmark
        {
            Name = name,
            Url = url,
            Category = category
        });
        CommandHelper.ShowSuccessMessage(["Bookmark successfully added!"]);
    }

    public IEnumerable<Bookmark> GetAll()
    {
        return ExistingBookmarks;
    }

    public IEnumerable<string> GetCategories()
    {
        return Categories;
    }

    public bool ChangeBookmarkCategory(string url, string category)
    {
        Bookmark? bookmark = ExistingBookmarks?.FirstOrDefault(b => string.Equals(b.Url, url, StringComparison.OrdinalIgnoreCase));

        if (bookmark is not null)
        {
            bookmark.Category = category;
            return true;
        }

        return false;
    }

    public Bookmark? GetBookmark(string bookmarkName)
    {
        return ExistingBookmarks?.FirstOrDefault(b => string.Equals(b.Name, bookmarkName, StringComparison.OrdinalIgnoreCase));
    }

    public void Import(IEnumerable<Bookmark> bookmarks, bool merge)
    {
        if (!bookmarks.Any() || bookmarks is null)
        {
            return;
        }

        if (!merge)
        {
            ExistingBookmarks.AddRange(bookmarks);
        }
        if (merge)
        {
            foreach (Bookmark bookmark in bookmarks)
            {
                var existingbookmark =
                ExistingBookmarks.Find(e => string.Equals(e.Url, bookmark.Url, StringComparison.OrdinalIgnoreCase));

                if (existingbookmark is not null)
                {
                    existingbookmark.Name = bookmark.Name;
                }
                else
                {
                    ExistingBookmarks.Add(bookmark);
                }
            }
        }

        PrintBookmarks();
    }

    public async void ExportBookmarksAsync(IEnumerable<Bookmark> bookmarks, FileInfo outputFile, CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputFile.FullName, json, cancellationToken);
    }

    public void ExportBookmark(Bookmark bookmark, string outputFilePath, StreamWriter streamWriter)
    {
        streamWriter.WriteLine(JsonSerializer.Serialize(bookmark));
    }

    public BookMarkConflictModel? Import(Bookmark bookmark, bool merge)
    {
        Bookmark? conflict = ExistingBookmarks.FirstOrDefault(b =>
            b.Url == bookmark.Url && b.Name != bookmark.Name);

        if (conflict is not null && merge)
        {
            var conflictModel = new BookMarkConflictModel
            {
                OriginalName = conflict.Name,
                UpdatedName = bookmark.Name,
                Url = bookmark.Url
            };
            conflict.Name = bookmark.Name;

            return conflictModel;
        }
        else
        {
            ExistingBookmarks.Add(bookmark);
            return null;
        }
    }

    public IEnumerable<Bookmark> GetBookmarksByCategory(string category)
    {
        return ExistingBookmarks.Where(b => string.Equals(b.Category, category));
    }

    public void ClearAll()
    {
        ExistingBookmarks.Clear();
    }

    public void PrintBookmarks()
    {
        foreach (var bookmark in ExistingBookmarks)
        {
            Console.WriteLine(bookmark.ToString());
        }
    }

}
