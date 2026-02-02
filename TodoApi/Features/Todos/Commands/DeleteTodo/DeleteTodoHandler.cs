namespace TodoApi.Features.Todos.Commands.DeleteTodo;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

public class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteTodoHandler> _logger;

    public DeleteTodoHandler(ApplicationDbContext context, ILogger<DeleteTodoHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == request.TodoId && t.UserId == request.UserId, cancellationToken);

        if (todo == null)
        {
            _logger.LogWarning("Todo {TodoId} not found for user {UserId}", request.TodoId, request.UserId);
            return false;
        }

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted todo {TodoId} for user {UserId}", request.TodoId, request.UserId);

        return true;
    }
}
