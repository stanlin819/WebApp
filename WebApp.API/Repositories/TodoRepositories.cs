using Microsoft.EntityFrameworkCore;
using WebApp.API.Data;

namespace WebApp.API.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    public TodoRepository(AppDbContext context) => _context = context;
    public async Task<Todo> Add(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }
    public async Task Delete(Todo todo)
    {
        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
    }
    public async Task<Todo> Get(int id)
    {
        var t = await _context.Todos.FindAsync(id);
        return t;
    }
    public async Task<List<Todo>> GetUserTodo(int userId)
    {
        return await _context.Todos.Where(t => t.UserId == userId).ToListAsync();

    }
    public async Task Update(Todo todo)
    {
        _context.Todos.Update(todo);
        if(await _context.Todos.FindAsync(todo.Id) != null)
            await _context.SaveChangesAsync();
    }
}