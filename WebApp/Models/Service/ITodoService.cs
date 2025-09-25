using WebApp.Models.TodoModel;

namespace WebApp.Models.Service;

public interface ITodoService
{
    Task<Todo> AddTodo(Todo t);

    Task<List<Todo>> GetTodos(int userId);

    Task<Todo> GetTodo(int todoId);
    Task DeleteTodo(int id);
    Task UpdateTodo(Todo todo);
}