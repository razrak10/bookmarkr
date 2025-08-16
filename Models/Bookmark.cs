namespace bookmarkr;

public class Bookmark
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Url { get; set; }

    public string? Category { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
