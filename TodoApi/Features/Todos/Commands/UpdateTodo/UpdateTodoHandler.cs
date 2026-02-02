namespace TodoApi.Features.Todos.Commands.UpdateTodo;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Features.Todos.Commands.CreateTodo;

public class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, TodoResponse?>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateTodoHandler> _logger;

    public UpdateTodoHandler(ApplicationDbContext context, ILogger<UpdateTodoHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TodoResponse?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == request.TodoId && t.UserId == request.UserId, cancellationToken);

        if (todo == null)
        {
            _logger.LogWarning("Todo {TodoId} not found for user {UserId}", request.TodoId, request.UserId);
            return null;
        }

        todo.Title = request.Title.Trim();
        todo.Description = request.Description?.Trim();
        todo.IsCompleted = request.IsCompleted;
        todo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated todo {TodoId} for user {UserId}", request.TodoId, request.UserId);

        return new TodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt
        };
    }
}
