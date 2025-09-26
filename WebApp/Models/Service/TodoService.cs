using Org.BouncyCastle.Asn1.Cms;
using WebApp.Models.Repositories;
using WebApp.Models.TodoModel;

namespace WebApp.Models.Service;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repo;

    public TodoService(ITodoRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Todo>> GetTodos(int userId)
    {
        var todos = await _repo.GetUserTodo(userId);
        return todos;
    }

    public async Task<Todo> GetTodo(int todoId)
    {
        return await _repo.Get(todoId);
    }

    public async Task<Todo> AddTodo(Todo t)
    {
        return await _repo.Add(t);
    }

    public async Task DeleteTodo(int id)
    {
        var t = await _repo.Get(id);
        await _repo.Delete(t);
    }

    public async Task UpdateTodo(Todo todo)
    {
        await _repo.Update(todo);
    }

    public async Task<Todo> Toggle(int todoId)
    {
        var todo = await _repo.Get(todoId);
        if (todo == null)
            return null;
        todo.done = (todo.done) ? false : true;
        await _repo.Update(todo);
        return todo;

    }
}