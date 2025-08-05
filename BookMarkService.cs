using System;

namespace bookmarkr;

public class BookMarkService
{
    private readonly List<Bookmark> _bookmarks = new List<Bookmark>
    {
        new Bookmark {
            Name = "First",
            Category = "Read later",
            Url = "https://www.lol.com"
        },
        new Bookmark {
            Name = "Second",
            Category = "Tech books",
            Url = "https://www.seocnd.com"
        },
    };

    public List<Bookmark> Bookmarks => _bookmarks;

    public void AddLink(string name, string url, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            // Helper.ShowErrorMessage(["the `name` for the link is not provided. The expected sytnax is:", "bookmarkr link add <name> <url>"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            // Helper.ShowErrorMessage(["the `url` for the link is not provided. The expected sytnax is:", "bookmarkr link add <name> <url>"]);
            return;
        }
        if (Bookmarks.Any(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            // Helper.ShowErrorMessage(["A link with the name `{name}` already exists. It will thus not be added",
            // $"To update the existing link, use the command: bookmarkr link update `{name}` `{url}`"]);
            return;
        }

        Bookmarks.Add(new Bookmark
        {
            Name = name,
            Url = url,
            Category = category
        });
        // Helper.ShowSuccessMessage(["Bookmark successfully added!"]);
    }

    public IReadOnlyCollection<Bookmark> GetAll()
    {
        return this.Bookmarks;
    }

    public void ImportBookmarks(IEnumerable<Bookmark> bookmarks)
    {
        if (bookmarks is not null && bookmarks.Any())
            Bookmarks.AddRange(bookmarks);
    }
}
