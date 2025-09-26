using Moq;
using WebApp.Models.Repositories;
using WebApp.Models.Service;
using WebApp.Models.TodoModel;
using Xunit;

namespace WebApp.Tests.Services
{
    public class TodoServiceTests
    {
        private readonly Mock<ITodoRepository> _mockRepo;
        private readonly TodoService _service;

        public TodoServiceTests()
        {
            _mockRepo = new Mock<ITodoRepository>();
            _service = new TodoService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetTodos_ShouldReturnTodos()
        {
            // Arrange
            var userId = 1;
            var expected = new List<Todo> { new Todo { Id = 1, text = "Test" } };
            _mockRepo.Setup(r => r.GetUserTodo(userId)).ReturnsAsync(expected);

            // Act
            var result = await _service.GetTodos(userId);

            // Assert
            Assert.Equal(expected, result);
            _mockRepo.Verify(r => r.GetUserTodo(userId), Times.Once);
        }

        [Fact]
        public async Task GetTodo_ShouldReturnTodo()
        {
            // Arrange
            var todoId = 1;
            var expected = new Todo { Id = todoId, text = "Test" };
            _mockRepo.Setup(r => r.Get(todoId)).ReturnsAsync(expected);

            // Act
            var result = await _service.GetTodo(todoId);

            // Assert
            Assert.Equal(expected, result);
            _mockRepo.Verify(r => r.Get(todoId), Times.Once);
        }

        [Fact]
        public async Task AddTodo_ShouldReturnAddedTodo()
        {
            // Arrange
            var todo = new Todo { Id = 1, text = "New Todo" };
            _mockRepo.Setup(r => r.Add(todo)).ReturnsAsync(todo);

            // Act
            var result = await _service.AddTodo(todo);

            // Assert
            Assert.Equal(todo, result);
            _mockRepo.Verify(r => r.Add(todo), Times.Once);
        }

        [Fact]
        public async Task DeleteTodo_ShouldCallDelete()
        {
            // Arrange
            var todoId = 1;
            var todo = new Todo { Id = todoId, text = "Delete Me" };
            _mockRepo.Setup(r => r.Get(todoId)).ReturnsAsync(todo);
            _mockRepo.Setup(r => r.Delete(todo)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteTodo(todoId);

            // Assert
            _mockRepo.Verify(r => r.Get(todoId), Times.Once);
            _mockRepo.Verify(r => r.Delete(todo), Times.Once);
        }

        [Fact]
        public async Task UpdateTodo_ShouldCallUpdate()
        {
            // Arrange
            var todo = new Todo { Id = 1, text = "Update Me" };
            _mockRepo.Setup(r => r.Update(todo)).Returns(Task.CompletedTask);

            // Act
            await _service.UpdateTodo(todo);

            // Assert
            _mockRepo.Verify(r => r.Update(todo), Times.Once);
        }

        [Fact]
        public async Task Toggle_ShouldReturnUpdatedTodo_WhenTodoExists()
        {
            // Arrange
            int todoId = 1;
            var todo = new Todo { Id = todoId, done = false };

            _mockRepo.Setup(r => r.Get(todoId)).ReturnsAsync(todo);
            _mockRepo.Setup(r => r.Update(todo)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.Toggle(todoId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(todoId, result.Id);
            Assert.True(result.done); // 原本 false，應該被切換成 true
            _mockRepo.Verify(r => r.Get(todoId), Times.Once);
            _mockRepo.Verify(r => r.Update(todo), Times.Once);
        }

        [Fact]
        public async Task Toggle_ShouldReturnNull_WhenTodoDoesNotExist()
        {
            // Arrange
            int todoId = 99;

            _mockRepo.Setup(r => r.Get(todoId)).ReturnsAsync((Todo)null);

            // Act
            var result = await _service.Toggle(todoId);

            // Assert
            Assert.Null(result);
            _mockRepo.Verify(r => r.Get(todoId), Times.Once);
            _mockRepo.Verify(r => r.Update(It.IsAny<Todo>()), Times.Never);
        }
    }
}
