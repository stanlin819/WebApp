public interface ITodoService
{
    Task<List<Todo>> GetTodos(int userId);
    Task<Todo> GetTodo(int id);
    Task<Todo> AddTodo(Todo todo);
    Task DeleteTodo(int id);
    Task UpdateTodo(Todo todo);
    Task<Todo> Toggle(int id);
}