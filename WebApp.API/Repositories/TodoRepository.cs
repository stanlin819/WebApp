using Microsoft.EntityFrameworkCore;
using WebApp.API.Data;

namespace WebApp.API.Repositories;

/// <summary>
/// 待辦事項資料庫操作存儲庫，實作待辦事項相關的CRUD操作
/// </summary>
public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// 初始化TodoRepository實例
    /// </summary>
    /// <param name="context">AppDbContext</param>
    public TodoRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// 新增待辦事項到資料庫
    /// </summary>
    /// <param name="todo">要新增的待辦事項物件</param>
    /// <returns>新增後的待辦事項物件</returns>
    public async Task<Todo> Add(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    /// <summary>
    /// 從資料庫刪除待辦事項
    /// </summary>
    /// <param name="todo">要刪除的待辦事項物件</param>
    /// <returns></returns>
    public async Task Delete(Todo todo)
    {
        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 根據待辦事項ID取得特定待辦事項
    /// </summary>
    /// <param name="id">待辦事項ID</param>
    /// <returns>找到的待辦事項物件，若不存在則返回 null</returns>
    public async Task<Todo> Get(int id)
    {
        var t = await _context.Todos.FindAsync(id);
        return t;
    }

    /// <summary>
    /// 取得特定用戶的所有待辦事項
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <returns>該用戶的待辦事項列表</returns>
    public async Task<List<Todo>> GetUserTodo(int userId)
    {
        return await _context.Todos.Where(t => t.UserId == userId).ToListAsync();

    }

    /// <summary>
    /// 更新待辦事項資訊，僅在待辦事項存在時執行更新
    /// </summary>
    /// <param name="todo"></param>
    /// <returns></returns>
    public async Task Update(Todo todo)
    {
        _context.Todos.Update(todo);
        // 檢查待辦事項是否存在再執行保存
        if (await _context.Todos.FindAsync(todo.Id) != null)
            await _context.SaveChangesAsync();
    }
}