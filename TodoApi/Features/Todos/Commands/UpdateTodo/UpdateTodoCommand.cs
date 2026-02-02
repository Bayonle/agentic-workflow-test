namespace TodoApi.Features.Todos.Commands.UpdateTodo;

using MediatR;
using TodoApi.Features.Todos.Commands.CreateTodo;

/// <summary>
/// Command to update an existing todo.
/// </summary>
public record UpdateTodoCommand : IRequest<TodoResponse?>
{
    public string UserId { get; init; } = string.Empty;
    public Guid TodoId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
}
