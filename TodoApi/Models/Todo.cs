namespace TodoApi.Models;

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a todo item owned by a user.
/// </summary>
public class Todo
{
    /// <summary>
    /// Unique identifier for the todo.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The todo title (required, max 200 chars).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description (max 1000 chars).
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the todo is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// When the todo was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the todo was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Foreign key to the owning user.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the owning user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public IdentityUser User { get; set; } = null!;
}
