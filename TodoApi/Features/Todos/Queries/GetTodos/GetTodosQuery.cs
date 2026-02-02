namespace TodoApi.Features.Todos.Queries.GetTodos;

using MediatR;
using TodoApi.Features.Todos.Commands.CreateTodo;

/// <summary>
/// Query to get all todos for a user.
/// </summary>
public record GetTodosQuery : IRequest<IEnumerable<TodoResponse>>
{
    public string UserId { get; init; } = string.Empty;
    public bool? Completed { get; init; }
}
