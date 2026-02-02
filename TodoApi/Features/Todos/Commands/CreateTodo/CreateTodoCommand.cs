namespace TodoApi.Features.Todos.Commands.CreateTodo;

using MediatR;

/// <summary>
/// Command to create a new todo.
/// </summary>
public record CreateTodoCommand : IRequest<TodoResponse>
{
    /// <summary>
    /// The authenticated user's ID (set by controller).
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The todo title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Response DTO for todo operations.
/// </summary>
public record TodoResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
