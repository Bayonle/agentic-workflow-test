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
