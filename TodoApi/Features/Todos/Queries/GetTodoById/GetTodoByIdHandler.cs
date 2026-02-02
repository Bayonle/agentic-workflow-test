namespace TodoApi.Features.Todos.Queries.GetTodoById;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Features.Todos.Commands.CreateTodo;

public class GetTodoByIdHandler : IRequestHandler<GetTodoByIdQuery, TodoResponse?>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetTodoByIdHandler> _logger;

    public GetTodoByIdHandler(ApplicationDbContext context, ILogger<GetTodoByIdHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TodoResponse?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _context.Todos
            .Where(t => t.Id == request.TodoId && t.UserId == request.UserId)
            .Select(t => new TodoResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (todo == null)
        {
            _logger.LogWarning("Todo {TodoId} not found for user {UserId}", request.TodoId, request.UserId);
        }

        return todo;
    }
}
