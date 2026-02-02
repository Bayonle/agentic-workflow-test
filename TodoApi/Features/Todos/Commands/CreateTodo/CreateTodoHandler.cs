namespace TodoApi.Features.Todos.Commands.CreateTodo;

using MediatR;
using TodoApi.Data;
using TodoApi.Models;

/// <summary>
/// Handles CreateTodoCommand.
/// </summary>
public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, TodoResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateTodoHandler> _logger;

    public CreateTodoHandler(ApplicationDbContext context, ILogger<CreateTodoHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TodoResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            IsCompleted = false,
            CreatedAt = now,
            UpdatedAt = now,
            UserId = request.UserId
        };

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created todo {TodoId} for user {UserId}", todo.Id, request.UserId);

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
