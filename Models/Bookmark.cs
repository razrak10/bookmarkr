using System;
using System.ComponentModel.DataAnnotations;

namespace bookmarkr;

public record Bookmark
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(2048)]
    public required string Url { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Category { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
