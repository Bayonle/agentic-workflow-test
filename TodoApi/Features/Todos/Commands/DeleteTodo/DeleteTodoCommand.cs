namespace TodoApi.Features.Todos.Commands.DeleteTodo;

using MediatR;

/// <summary>
/// Command to delete a todo.
/// </summary>
public record DeleteTodoCommand : IRequest<bool>
{
    public string UserId { get; init; } = string.Empty;
    public Guid TodoId { get; init; }
}
