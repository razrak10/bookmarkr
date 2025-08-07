using System;

namespace bookmarkr;

public record Bookmark
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string Category { get; set; }
}
