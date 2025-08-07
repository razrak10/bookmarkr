using System;

namespace bookmarkr.Models;

public record BookMarkConflictModel
{
    public string? OriginalName { get; set; }

    public string? UpdatedName { get; set; }

    public string? Url { get; set; }
}
