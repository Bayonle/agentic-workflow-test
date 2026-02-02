namespace TodoApi.Features.Todos.Queries.GetTodos;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Features.Todos.Commands.CreateTodo;

public class GetTodosHandler : IRequestHandler<GetTodosQuery, IEnumerable<TodoResponse>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetTodosHandler> _logger;

    public GetTodosHandler(ApplicationDbContext context, ILogger<GetTodosHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TodoResponse>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Todos
            .Where(t => t.UserId == request.UserId);

        // Apply optional filter
        if (request.Completed.HasValue)
        {
            query = query.Where(t => t.IsCompleted == request.Completed.Value);
        }

        var todos = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} todos for user {UserId}", todos.Count, request.UserId);

        return todos;
    }
}
