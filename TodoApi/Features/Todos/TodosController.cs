namespace TodoApi.Features.Todos;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Features.Todos.Commands.CreateTodo;
using TodoApi.Features.Todos.Commands.UpdateTodo;
using TodoApi.Features.Todos.Commands.DeleteTodo;
using TodoApi.Features.Todos.Queries.GetTodos;
using TodoApi.Features.Todos.Queries.GetTodoById;

/// <summary>
/// Manages todo items for authenticated users.
/// </summary>
[ApiController]
[Route("api/todos")]
[Produces("application/json")]
[Authorize]
[Tags("Todos")]
public class TodosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TodosController> _logger;

    public TodosController(IMediator mediator, ILogger<TodosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new todo item.
    /// </summary>
    /// <param name="command">Todo details</param>
    /// <returns>201 Created with the new todo</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTodo([FromBody] CreateTodoCommand command)
    {
        var userId = GetUserId();
        var commandWithUser = command with { UserId = userId };

        var result = await _mediator.Send(commandWithUser);

        return CreatedAtAction(nameof(GetTodoById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets all todos for the authenticated user.
    /// </summary>
    /// <param name="completed">Optional filter by completion status</param>
    /// <returns>200 OK with list of todos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTodos([FromQuery] bool? completed = null)
    {
        var userId = GetUserId();
        var query = new GetTodosQuery { UserId = userId, Completed = completed };

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific todo by ID.
    /// </summary>
    /// <param name="id">The todo ID</param>
    /// <returns>200 OK with the todo, or 404 if not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTodoById(Guid id)
    {
        var userId = GetUserId();
        var query = new GetTodoByIdQuery { UserId = userId, TodoId = id };

        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Todo not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing todo.
    /// </summary>
    /// <param name="id">The todo ID</param>
    /// <param name="command">Updated todo details</param>
    /// <returns>200 OK with updated todo, or 404 if not found</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateTodo(Guid id, [FromBody] UpdateTodoCommand command)
    {
        var userId = GetUserId();
        var commandWithUser = command with { UserId = userId, TodoId = id };

        var result = await _mediator.Send(commandWithUser);

        if (result == null)
        {
            return NotFound(new { message = "Todo not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Deletes a todo.
    /// </summary>
    /// <param name="id">The todo ID</param>
    /// <returns>204 No Content on success, or 404 if not found</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTodo(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteTodoCommand { UserId = userId, TodoId = id };

        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(new { message = "Todo not found" });
        }

        return NoContent();
    }

    private string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("User ID claim not found in JWT token");
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
