namespace WebApp.API.Repositories;
public interface ITodoRepository
{
    Task<Todo> Add(Todo todo);
    Task Delete(Todo todo);
    Task<Todo> Get(int id);
    Task<List<Todo>> GetUserTodo(int userId);
    Task Update(Todo todo);
}