using Xunit;
using Moq;
using WebApp.Models.Service;
using WebApp.Models.TodoModel;
using WebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TodoAPIControllerTests
{
    private readonly Mock<ITodoService> _mockService;
    private readonly TodoAPIController _controller;

    public TodoAPIControllerTests()
    {
        _mockService = new Mock<ITodoService>();
        _controller = new TodoAPIController(_mockService.Object);
    }

    [Fact]
    public async Task AddTodo_ReturnsOk_WithTodo()
    {
        // Arrange
        var todo = new Todo { UserId = 1, text = "Test Task" };

        _mockService.Setup(s => s.AddTodo(todo))
                .ReturnsAsync(todo);

        // Act
        var result = await _controller.AddTodo(todo);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTodo = Assert.IsType<Todo>(okResult.Value);
        Assert.Equal(todo.text, returnedTodo.text);
    }

    [Fact]
    public async Task GetTodos_ReturnsOk_WhenTodosExist()
    {
        // Arrange
        int userId = 1;
        var todos = new List<Todo> { new Todo { UserId = 1, text = "Task1" } };

        _mockService.Setup(s => s.GetTodos(userId))
                    .ReturnsAsync(todos);

        // Act
        var result = await _controller.GetTodos(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTodos = Assert.IsAssignableFrom<IEnumerable<Todo>>(okResult.Value);
        Assert.Single(returnedTodos);
    }

    [Fact]
    public async Task GetTodos_ReturnsNoContent_WhenNoTodos()
    {
        // Arrange
        int userId = 1;
        _mockService.Setup(s => s.GetTodos(userId))
                    .ReturnsAsync((List<Todo>)null);

        // Act
        var result = await _controller.GetTodos(userId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTodo_ReturnsOk_WhenTodoExists()
    {
        // Arrange
        int todoId = 1;
        var todo = new Todo { Id = todoId, text = "Task" };

        _mockService.Setup(s => s.GetTodo(todoId))
                    .ReturnsAsync(todo);
        _mockService.Setup(s => s.DeleteTodo(todoId))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteTodo(todoId);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteTodo_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        int todoId = 1;
        _mockService.Setup(s => s.GetTodo(todoId))
                    .ReturnsAsync((Todo)null);

        // Act
        var result = await _controller.DeleteTodo(todoId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Toggle_ReturnsOkWithTodo_WhenTodoExists()
    {
        // Arrange
        int todoId = 1;
        var todo = new Todo { Id = todoId, done = false };

        _mockService.Setup(s => s.GetTodo(todoId))
                    .ReturnsAsync(todo);
        _mockService.Setup(s => s.UpdateTodo(todo))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Toggle(todoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTodo = Assert.IsType<Todo>(okResult.Value);
        Assert.True(returnedTodo.done);
    }

    [Fact]
    public async Task Toggle_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        int todoId = 1;
        _mockService.Setup(s => s.GetTodo(todoId))
                    .ReturnsAsync((Todo)null);

        // Act
        var result = await _controller.Toggle(todoId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
