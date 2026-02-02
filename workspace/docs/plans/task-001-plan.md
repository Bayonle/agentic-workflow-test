---
task: task-001
feature: Creating a Todo
status: draft
created: 2026-02-02
author: Architect Agent
prd: docs/specs/task-001-prd.md
revision: 2
---

# Technical Implementation Plan: Todo CRUD API (MediatR + FluentValidation)

## Overview

Implement a complete CRUD API for Todo items using **CQRS pattern with MediatR**, **FluentValidation** for validation, and **EF Core** for data access. This architecture provides clean separation of concerns and follows enterprise patterns.

## Architecture

### Layer Structure

```
TodoApi/
├── Models/
│   └── Todo.cs                           # Domain entity
├── Features/
│   └── Todos/
│       ├── Commands/
│       │   ├── CreateTodo/
│       │   │   ├── CreateTodoCommand.cs       # Command
│       │   │   ├── CreateTodoValidator.cs     # FluentValidation
│       │   │   └── CreateTodoHandler.cs       # MediatR handler
│       │   ├── UpdateTodo/
│       │   │   ├── UpdateTodoCommand.cs
│       │   │   ├── UpdateTodoValidator.cs
│       │   │   └── UpdateTodoHandler.cs
│       │   └── DeleteTodo/
│       │       ├── DeleteTodoCommand.cs
│       │       └── DeleteTodoHandler.cs
│       ├── Queries/
│       │   ├── GetTodos/
│       │   │   ├── GetTodosQuery.cs
│       │   │   └── GetTodosHandler.cs
│       │   └── GetTodoById/
│       │       ├── GetTodoByIdQuery.cs
│       │       └── GetTodoByIdHandler.cs
│       └── TodosController.cs                 # Slim controller
└── Data/
    └── ApplicationDbContext.cs                # EF Core context
```

### Design Pattern: CQRS with MediatR

**Controller → MediatR → Command/Query Handler → EF Core DbContext**

Benefits:
- Clear separation of commands (writes) and queries (reads)
- Each operation is a single-purpose class
- Easy to test in isolation
- Follows SOLID principles

## NuGet Packages Required

Add to `TodoApi.csproj`:

```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
```

## Implementation Steps

### Step 1: Create Domain Model

**File**: `TodoApi/Models/Todo.cs`

```csharp
namespace TodoApi.Models;

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a todo item owned by a user.
/// </summary>
public class Todo
{
    /// <summary>
    /// Unique identifier for the todo.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The todo title (required, max 200 chars).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description (max 1000 chars).
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the todo is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// When the todo was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the todo was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Foreign key to the owning user.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the owning user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public IdentityUser User { get; set; } = null!;
}
```

### Step 2: Update Database Context

**File**: `TodoApi/Data/ApplicationDbContext.cs`

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;  // Add this

namespace TodoApi.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add DbSet for Todos
    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Todo entity
        modelBuilder.Entity<Todo>(entity =>
        {
            // Primary key
            entity.HasKey(t => t.Id);

            // Indexes for performance
            entity.HasIndex(t => t.UserId)
                .HasDatabaseName("IX_Todos_UserId");

            entity.HasIndex(t => t.CreatedAt)
                .HasDatabaseName("IX_Todos_CreatedAt");

            // Foreign key relationship
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Required fields
            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Description)
                .HasMaxLength(1000);

            entity.Property(t => t.UserId)
                .IsRequired();
        });
    }
}
```

### Step 3: Create Database Migration

```bash
cd TodoApi
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet ef migrations add AddTodoEntity
dotnet ef database update
```

### Step 4: Create Todo Feature - Commands

#### 4.1 Create Todo Command

**File**: `TodoApi/Features/Todos/Commands/CreateTodo/CreateTodoCommand.cs`

```csharp
namespace TodoApi.Features.Todos.Commands.CreateTodo;

using MediatR;

/// <summary>
/// Command to create a new todo.
/// </summary>
public record CreateTodoCommand : IRequest<TodoResponse>
{
    /// <summary>
    /// The authenticated user's ID (set by controller).
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The todo title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Response DTO for todo operations.
/// </summary>
public record TodoResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

**File**: `TodoApi/Features/Todos/Commands/CreateTodo/CreateTodoValidator.cs`

```csharp
namespace TodoApi.Features.Todos.Commands.CreateTodo;

using FluentValidation;

/// <summary>
/// Validates CreateTodoCommand using FluentValidation.
/// </summary>
public class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .Length(1, 200)
            .WithMessage("Title must be between 1 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => x.Description != null);
    }
}
```

**File**: `TodoApi/Features/Todos/Commands/CreateTodo/CreateTodoHandler.cs`

```csharp
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
```

#### 4.2 Update Todo Command

**File**: `TodoApi/Features/Todos/Commands/UpdateTodo/UpdateTodoCommand.cs`

```csharp
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
```

**File**: `TodoApi/Features/Todos/Commands/UpdateTodo/UpdateTodoValidator.cs`

```csharp
namespace TodoApi.Features.Todos.Commands.UpdateTodo;

using FluentValidation;

public class UpdateTodoValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.TodoId)
            .NotEmpty()
            .WithMessage("Todo ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .Length(1, 200)
            .WithMessage("Title must be between 1 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => x.Description != null);
    }
}
```

**File**: `TodoApi/Features/Todos/Commands/UpdateTodo/UpdateTodoHandler.cs`

```csharp
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
```

#### 4.3 Delete Todo Command

**File**: `TodoApi/Features/Todos/Commands/DeleteTodo/DeleteTodoCommand.cs`

```csharp
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
```

**File**: `TodoApi/Features/Todos/Commands/DeleteTodo/DeleteTodoHandler.cs`

```csharp
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
```

### Step 5: Create Todo Feature - Queries

#### 5.1 Get Todos Query

**File**: `TodoApi/Features/Todos/Queries/GetTodos/GetTodosQuery.cs`

```csharp
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
```

**File**: `TodoApi/Features/Todos/Queries/GetTodos/GetTodosHandler.cs`

```csharp
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
```

#### 5.2 Get Todo By ID Query

**File**: `TodoApi/Features/Todos/Queries/GetTodoById/GetTodoByIdQuery.cs`

```csharp
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
```

**File**: `TodoApi/Features/Todos/Queries/GetTodoById/GetTodoByIdHandler.cs`

```csharp
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
```

### Step 6: Create Controller

**File**: `TodoApi/Features/Todos/TodosController.cs`

```csharp
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
```

### Step 7: Register Services in DI Container

**File**: `TodoApi/Program.cs`

Add after line 122 (after `AddScoped<IAuthService, AuthService>`):

```csharp
// Register MediatR (auto-discovers handlers in assembly)
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add validation behavior to MediatR pipeline
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### Step 8: Create Validation Pipeline Behavior

**File**: `TodoApi/Behaviors/ValidationBehavior.cs`

```csharp
namespace TodoApi.Behaviors;

using FluentValidation;
using MediatR;

/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### Step 9: Add Global Exception Handler

**File**: `TodoApi/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
namespace TodoApi.Middleware;

using FluentValidation;
using System.Net;
using System.Text.Json;

/// <summary>
/// Global exception handler that converts exceptions to appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        _logger.LogWarning(exception, "Validation error occurred");

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            message = "Validation failed",
            errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
    {
        _logger.LogWarning(exception, "Unauthorized access attempt");

        var response = new { message = exception.Message };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var response = new { message = "An error occurred processing your request" };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

**Update `Program.cs`** to add middleware (after line 128, before `app.UseHttpsRedirection()`):

```csharp
// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### Step 10: Create Tests

**File**: `TodoApi.Tests/Features/Todos/CreateTodoHandlerTests.cs`

```csharp
namespace TodoApi.Tests.Features.Todos;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.Data;
using TodoApi.Features.Todos.Commands.CreateTodo;
using Xunit;

public class CreateTodoHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly CreateTodoHandler _handler;

    public CreateTodoHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        var logger = new Mock<ILogger<CreateTodoHandler>>().Object;
        _handler = new CreateTodoHandler(_context, logger);
    }

    [Fact]
    public async Task Handle_ShouldCreateTodo()
    {
        // Arrange
        var command = new CreateTodoCommand
        {
            UserId = "test-user",
            Title = "Test Todo",
            Description = "Test Description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Todo", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.False(result.IsCompleted);

        var savedTodo = await _context.Todos.FindAsync(result.Id);
        Assert.NotNull(savedTodo);
        Assert.Equal("test-user", savedTodo.UserId);
    }

    [Fact]
    public async Task Handle_ShouldTrimWhitespace()
    {
        // Arrange
        var command = new CreateTodoCommand
        {
            UserId = "test-user",
            Title = "  Padded Title  ",
            Description = "  Padded Description  "
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Padded Title", result.Title);
        Assert.Equal("Padded Description", result.Description);
    }
}
```

**File**: `TodoApi.Tests/Features/Todos/CreateTodoValidatorTests.cs`

```csharp
namespace TodoApi.Tests.Features.Todos;

using FluentValidation.TestHelper;
using TodoApi.Features.Todos.Commands.CreateTodo;
using Xunit;

public class CreateTodoValidatorTests
{
    private readonly CreateTodoValidator _validator;

    public CreateTodoValidatorTests()
    {
        _validator = new CreateTodoValidator();
    }

    [Fact]
    public void Should_HaveError_WhenTitleIsEmpty()
    {
        var command = new CreateTodoCommand { UserId = "user", Title = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_HaveError_WhenTitleIsTooLong()
    {
        var command = new CreateTodoCommand
        {
            UserId = "user",
            Title = new string('a', 201)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        var command = new CreateTodoCommand
        {
            UserId = "user",
            Title = "Valid Title"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

## Files Summary

### Files to Create (19 new files)

**Domain & Data:**
1. `TodoApi/Models/Todo.cs`
2. `TodoApi/Data/ApplicationDbContext.cs` (modify)

**Commands:**
3. `TodoApi/Features/Todos/Commands/CreateTodo/CreateTodoCommand.cs`
4. `TodoApi/Features/Todos/Commands/CreateTodo/CreateTodoValidator.cs`
5. `TodoApi/Features/Todos/Commands/CreateTodo/CreateTodoHandler.cs`
6. `TodoApi/Features/Todos/Commands/UpdateTodo/UpdateTodoCommand.cs`
7. `TodoApi/Features/Todos/Commands/UpdateTodo/UpdateTodoValidator.cs`
8. `TodoApi/Features/Todos/Commands/UpdateTodo/UpdateTodoHandler.cs`
9. `TodoApi/Features/Todos/Commands/DeleteTodo/DeleteTodoCommand.cs`
10. `TodoApi/Features/Todos/Commands/DeleteTodo/DeleteTodoHandler.cs`

**Queries:**
11. `TodoApi/Features/Todos/Queries/GetTodos/GetTodosQuery.cs`
12. `TodoApi/Features/Todos/Queries/GetTodos/GetTodosHandler.cs`
13. `TodoApi/Features/Todos/Queries/GetTodoById/GetTodoByIdQuery.cs`
14. `TodoApi/Features/Todos/Queries/GetTodoById/GetTodoByIdHandler.cs`

**Controller & Infrastructure:**
15. `TodoApi/Features/Todos/TodosController.cs`
16. `TodoApi/Behaviors/ValidationBehavior.cs`
17. `TodoApi/Middleware/ExceptionHandlingMiddleware.cs`

**Tests:**
18. `TodoApi.Tests/Features/Todos/CreateTodoHandlerTests.cs`
19. `TodoApi.Tests/Features/Todos/CreateTodoValidatorTests.cs`

### Files to Modify (2 files)
1. `TodoApi/Program.cs` - Register MediatR, FluentValidation, middleware
2. `TodoApi.csproj` - Add NuGet packages

## Benefits of This Architecture

### 1. CQRS with MediatR
- ✅ Clear separation of commands (writes) and queries (reads)
- ✅ Each operation is a single-purpose class
- ✅ Easy to test handlers in isolation
- ✅ Can add cross-cutting concerns (validation, logging) via pipeline behaviors

### 2. FluentValidation
- ✅ Validation rules in dedicated classes (not scattered in DTOs)
- ✅ Reusable and composable rules
- ✅ Easy to test validation logic independently
- ✅ Automatic validation via MediatR pipeline

### 3. EF Core Best Practices
- ✅ DbContext with proper configuration
- ✅ Indexes for performance
- ✅ Projection to DTOs in queries (don't return entities)
- ✅ Async/await throughout

### 4. Clean Architecture
- ✅ Slim controllers (just route to MediatR)
- ✅ Business logic in handlers
- ✅ No service layer needed (handlers are single-purpose)
- ✅ Testable components

## Security Considerations

- ✅ All endpoints protected with `[Authorize]`
- ✅ UserId from JWT claims (cannot be spoofed)
- ✅ User ownership enforced in every handler
- ✅ Validation via FluentValidation
- ✅ Global exception handler (don't leak details)

## Testing Strategy

- ✅ Handler unit tests with in-memory database
- ✅ Validator unit tests with FluentValidation test helpers
- ✅ Integration tests via WebApplicationFactory
- ✅ Swagger manual testing

## Next Steps

After plan approval:
1. **Engineer** installs NuGet packages
2. **Engineer** implements all files according to plan
3. **Engineer** creates migration and updates database
4. **Engineer** runs tests
5. **Engineer** creates PR
6. **QA** tests functionality
7. **DevOps** deploys to production

---

**Status**: Ready for human approval
**Reviewer**: @human
