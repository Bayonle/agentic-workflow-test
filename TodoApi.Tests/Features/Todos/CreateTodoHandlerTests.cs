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
