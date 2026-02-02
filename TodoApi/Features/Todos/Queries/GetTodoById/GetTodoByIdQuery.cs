namespace TodoApi.Features.Todos.Queries.GetTodoById;

using MediatR;
using TodoApi.Features.Todos.Commands.CreateTodo;

/// <summary>
/// Query to get a single todo by ID.
/// </summary>
public record GetTodoByIdQuery : IRequest<TodoResponse?>
{
    public string UserId { get; init; } = string.Empty;
    public Guid TodoId { get; init; }
}
